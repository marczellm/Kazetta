using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using Kazetta.ViewModel;
using System.Windows.Controls;

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
                        || (p.IsVocalistToo && targetTeacher.IsVocalist)))
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
                var p = (Person)dropInfo.Data;
                D.Students.Remove(p);
                D.Groups.RemoveAll(e => e.Persons.Contains(p));
            }
            else if (target.Name.StartsWith("kcs"))
            {
                var targetCollection = (ObservableCollection2<Group>)dropInfo.TargetCollection;
                var g = (Group)dropInfo.Data;
                int i = dropInfo.InsertIndex;
                var targetTeacher = (Person)((HeaderedItemsControl)target).Header;
                var p = g.Persons[0];
                if (p.Instrument == targetTeacher.Instrument)
                {
                    if (p.Teacher != null)                    
                        sourceCollection()[dropInfo.DragInfo.SourceIndex] = new Group();                    
                    foreach (var q in g.Persons)
                    {
                        q.TimeSlot = i;                        
                        q.Teacher = targetTeacher;

                        if (q.VocalTimeSlot == i)
                        {
                            int j = D.Teachers.IndexOf(q.VocalTeacher);
                            q.VocalTeacher = null;
                            if (j >= 0)
                                D.Schedule[j][i] = new Group();
                        }
                    }         
                }
                else if (p.IsVocalistToo && targetTeacher.IsVocalist)
                {
                    if (p.VocalTeacher != null)
                        sourceCollection()[dropInfo.DragInfo.SourceIndex] = new Group();
                    p.VocalTimeSlot = i;
                    p.VocalTeacher = targetTeacher;

                    if (p.TimeSlot == i)
                    {
                        int j = D.Teachers.IndexOf(p.Teacher);
                        p.Teacher = null;
                        if (j >= 0)
                            D.Schedule[j][i] = new Group();
                    }
                }
                else return;

                targetCollection[dropInfo.InsertIndex % targetCollection.Count] = g;
            }
            else if (source.Name.StartsWith("kcs") && target.Name == "nokcs")
            {
                var g = (Group)dropInfo.Data;
                var sourceTeacher = (Person)((HeaderedItemsControl)source).Header;

                foreach (Person p in g.Persons)
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
