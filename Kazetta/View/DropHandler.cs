using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using Kazetta.ViewModel;
using System.Windows.Controls;
using System;

namespace Kazetta.View
{
    class DropHandler : FrameworkElement, IDropTarget
    {
        private ViewModel.MainWindow D => (ViewModel.MainWindow)DataContext;
        /// <summary>
        /// Set where drops are allowed
        /// </summary>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            dropInfo.Effects = DragDropEffects.None;
            dropInfo.DropTargetAdorner = null;

            if (dropInfo.Data is Person p)
            {
                if (source.Name == "PeopleView" && (target.Name == "PeopleView" || target.Name == "AddOrRemovePersonButton"))
                    dropInfo.Effects = DragDropEffects.Move;
            }
            else if (dropInfo.Data is Group g && g.Persons.Length > 0)
            {
                if (target.Name.StartsWith("kcs"))
                {
                    var targetTeacher = (Person)((HeaderedItemsControl)target).Header;
                    p = g.Persons[0];
                    if (source != target && (p.Teacher == targetTeacher || p.VocalTeacher == targetTeacher))
                        return;
                    if (source.Name.StartsWith("kcs"))
                    {
                        var sourceTeacher = (Person)((HeaderedItemsControl)source).Header;
                        if (sourceTeacher.Instrument == targetTeacher.Instrument)
                        {
                            dropInfo.Effects = DragDropEffects.Move;
                            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        }
                    }
                    else if (source.Name == "nokcs" && (p.Instrument == targetTeacher.Instrument &&
                            ((g.Persons.Length > 1 && p.Pair == g.Persons[1]) || ((g.Persons.Length == 1 && p.Pair == null)))
                        || (p.IsVocalistToo && targetTeacher.Instrument == Instrument.Voice)))
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
            
            if (target.Name == "AddOrRemovePersonButton") {
                var p = (Person)dropInfo.Data;
                D.Students.Remove(p);
                D.Groups.RemoveAll(e => e.Persons.Contains(p));
            }
            else if (target.Name.StartsWith("kcs"))
            {
                Func<ObservableCollection2<Group>> sourceCollection = () => (ObservableCollection2<Group>)dropInfo.DragInfo.SourceCollection;
                var targetCollection = (ObservableCollection2<Group>)dropInfo.TargetCollection;

                var g = (Group)dropInfo.Data;
                int i = dropInfo.InsertIndex;
                var targetTeacher = (Person)((HeaderedItemsControl)target).Header;
                if (g.Persons[0].Instrument == targetTeacher.Instrument)
                {
                    var sourceTeacher = g.Persons[0].Teacher;
                    if (sourceTeacher != null)                    
                        sourceCollection()[dropInfo.DragInfo.SourceIndex] = new Group();                    
                    foreach (var p in g.Persons)
                    {
                        p.TimeSlot = i;                        
                        p.Teacher = targetTeacher;
                    }                    
                }
                else if (g.Persons[0] is Person p && p.IsVocalistToo && targetTeacher.Instrument == Instrument.Voice)
                {
                    p.VocalTimeSlot = i;
                    if (p.VocalTeacher != null)
                        sourceCollection()[dropInfo.DragInfo.SourceIndex] = new Group();
                    p.VocalTeacher = targetTeacher;
                }
                else return;
                
                targetCollection.RemoveAt(dropInfo.InsertIndex % targetCollection.Count);
                targetCollection.Insert(dropInfo.InsertIndex % targetCollection.Count, g);
            }
            else if (source.Name.StartsWith("kcs") && target.Name == "nokcs")
            {
                var sourceCollection = (ObservableCollection2<Group>)dropInfo.DragInfo.SourceCollection;
                var g = (Group)dropInfo.Data;
                var sourceTeacher = (Person)((HeaderedItemsControl)source).Header;

                foreach (Person p in g.Persons)
                {
                    if (p.Teacher == sourceTeacher)
                        p.Teacher = null;
                    else if (p.VocalTeacher == sourceTeacher)
                        p.VocalTeacher = null;
                }

                sourceCollection[dropInfo.DragInfo.SourceIndex] = new Group();
            }
        }
    }
}
