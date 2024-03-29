﻿using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using Kazetta.ViewModel;
using System.Windows.Controls;
using System.Collections.Generic;

namespace Kazetta.View
{
    class DropHandler : FrameworkElement, IDropTarget
    {
        private ViewModel.MainWindow D => (ViewModel.MainWindow)DataContext;

        void IDropTarget.DragEnter(IDropInfo dropInfo)
        {
            
        }

        void IDropTarget.DragLeave(IDropInfo dropInfo)
        {
            
        }

        /// <summary>
        /// Set where drops are allowed
        /// </summary>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            dropInfo.Effects = DragDropEffects.None;
            dropInfo.DropTargetAdorner = null;

            if (dropInfo.Data is Person)
            {
                if (source.Name == "PeopleView" && (target.Name == "PeopleView" || target.Name == "AddOrRemovePersonButton"))
                    dropInfo.Effects = DragDropEffects.Move;
            }
            else if (dropInfo.Data is Group g && g.Persons.Length > 0)
            {
                if (target.Name.StartsWith("kcs"))
                {
                    var targetTeacher = (Teacher)((HeaderedItemsControl)target).Header;
                    Student p = g.Persons[0];
                    if (source != target && (p.Teacher == targetTeacher || p.VocalTeacher == targetTeacher))
                        return;
                    if (dropInfo.InsertIndex >= ((IEnumerable<Group>)dropInfo.TargetCollection).Count())
                        return;
                    if (source.Name.StartsWith("kcs"))
                    {
                        if (targetTeacher.Instruments.Contains(p.Instrument) || (targetTeacher.IsVocalist && p.IsVocalistToo))
                        {
                            dropInfo.Effects = DragDropEffects.Move;
                            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        }
                    }
                    else if (g.Unscheduled && D.CanAssign(g, targetTeacher))
                    {
                        dropInfo.Effects = DragDropEffects.Move;
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    }
                }
                else if (target.Name == "nokcs")
                    dropInfo.Effects = DragDropEffects.Move;
            }
        }        

        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            ObservableCollection2<Group> sourceCollection() => (ObservableCollection2<Group>)dropInfo.DragInfo.SourceCollection;

            if (target.Name == "AddOrRemovePersonButton") {
                var p = (Student)dropInfo.Data;
                D.Students.Remove(p);
                D.Groups.RemoveAll(e => e.Persons.Contains(p));
            }
            else if (target.Name.StartsWith("kcs"))
            {
                var targetCollection = (ObservableCollection2<Group>)dropInfo.TargetCollection;
                int i = dropInfo.InsertIndex;
                var targetTeacher = (Teacher)((HeaderedItemsControl)target).Header;
                foreach (var p in targetCollection[i].Persons)
                {
                    if (p.Teacher == targetTeacher)
                    {
                        p.Teacher = null;
                        p.TimeSlot = -1;
                    }
                    else if (p.VocalTeacher == targetTeacher)
                    {
                        p.VocalTeacher = null;
                        p.VocalTimeSlot = -1;
                    }
                }                
                
                D.AssignTo((Group)dropInfo.Data,
                    source.Name == "nokcs" ? null : sourceCollection(),
                    targetCollection,
                    targetTeacher,
                    dropInfo.DragInfo.SourceIndex,
                    i,
                    true);
            }
            else if (source.Name.StartsWith("kcs") && target.Name == "nokcs")
            {
                var g = (Group)dropInfo.Data;
                var sourceTeacher = (Teacher)((HeaderedItemsControl)source).Header;

                foreach (Student p in g.Persons)
                {
                    if (p.Teacher == sourceTeacher)
                        p.Teacher = null;
                    else if (p.VocalTeacher == sourceTeacher)
                        p.VocalTeacher = null;
                }

                sourceCollection()[dropInfo.DragInfo.SourceIndex] = new Group();
            }
        }
    }
}
