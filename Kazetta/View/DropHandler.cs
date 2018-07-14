using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using Kazetta.ViewModel;

namespace Kazetta.View
{
    class DropHandler : FrameworkElement, IDropTarget
    {
        private ViewModel.MainWindow D => (ViewModel.MainWindow)DataContext;
        private DefaultDropHandler defaultDropHandler = new DefaultDropHandler();
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
                    Person targetTeacher = D.Teachers[int.Parse((string)target.Tag)];
                    p = g.Persons[0];
                    if (source != target && (p.Teacher == targetTeacher || p.VocalTeacher == targetTeacher))
                        return;
                    if (source.Name.StartsWith("kcs"))
                    {
                        Person sourceTeacher = D.Teachers[int.Parse((string)source.Tag)];
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
                var g = (Group)dropInfo.Data;
                int i = dropInfo.InsertIndex,
                    j = int.Parse((string)target.Tag);
                var teacher = D.Teachers[j];
                if (g.Persons[0].Instrument == teacher.Instrument)
                {
                    foreach (var p in g.Persons)
                    {
                        p.TimeSlot = i;
                        if (p.Teacher != null)
                        {
                            int k = D.Teachers.IndexOf(p.Teacher);
                            int m = D.Schedule[k].IndexOf(g);
                            D.Schedule[k].Remove(g);
                            D.Schedule[k].Insert(m, new Group());
                        }
                        p.Teacher = teacher;
                    }                    
                }
                else if (g.Persons[0] is Person p && p.IsVocalistToo && teacher.Instrument == Instrument.Voice)
                {
                    p.VocalTimeSlot = i;
                    if (p.VocalTeacher != null)
                    {
                        int k = D.Teachers.IndexOf(p.VocalTeacher);
                        int m = D.Schedule[k].IndexOf(g);
                        D.Schedule[k].Remove(g);
                        D.Schedule[k].Insert(m, new Group());
                    }
                    p.VocalTeacher = teacher;
                }
                else return;
                
                var kcs = (ObservableCollection2<Group>)dropInfo.TargetCollection;
                kcs.RemoveAt(dropInfo.InsertIndex % kcs.Count);
                kcs.Insert(dropInfo.InsertIndex % kcs.Count, g);

                if (g.Persons.Length > 1 || (g.Persons[0].Teacher != null && g.Persons[0].VocalTeacher != null))
                    D.Groups.Remove(g);
            }
            else if (source.Name.StartsWith("kcs") && target.Name == "nokcs")
            {
                int i = int.Parse((string)source.Tag);
                var g = (Group)dropInfo.Data;
                Person q = D.Teachers[i];
                foreach (Person p in g.Persons)
                {
                    if (p.Teacher == q)
                        p.Teacher = null;
                    else if (p.VocalTeacher == q)
                        p.VocalTeacher = null;
                }

                int j = D.Schedule[i].IndexOf(g);
                D.Schedule[i].Remove(g);
                D.Schedule[i].Insert(j, new Group());
            }
        }
    }
}
