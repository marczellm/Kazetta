using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;

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
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            dropInfo.Effects = DragDropEffects.None;

            if (dropInfo.Data is Person p)
            {
                if (source.Name == "PeopleView" && (target.Name == "PeopleView" || target.Name == "AddOrRemovePersonButton"))
                    dropInfo.Effects = DragDropEffects.Move;
                else if (p.Pinned)
                    D.StatusText = p + " le van rögzítve!";
                else if (target.Name.StartsWith("kcs"))
                {
                    Person targetTeacher = D.Teachers[int.Parse((string)target.Tag)];

                    if (source.Name.StartsWith("kcs"))
                    {
                        Person sourceTeacher = D.Teachers[int.Parse((string)source.Tag)];
                        if (sourceTeacher.Instrument == targetTeacher.Instrument)
                            dropInfo.Effects = DragDropEffects.Move;
                    }
                    else if (source.Name == "nokcs" && (p.Instrument == targetTeacher.Instrument
                        || (p.IsVocalistToo && targetTeacher.Instrument == Instrument.Voice)))
                        dropInfo.Effects = DragDropEffects.Move;
                }
                else if (target.Name == "nokcs")
                {
                    dropInfo.DropTargetAdorner = null;
                    dropInfo.Effects = DragDropEffects.Move;
                }
            }
        }        

        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            Person p = (Person)dropInfo.Data;
            if (target.Name == "AddOrRemovePersonButton") { 
                D.Students.Remove(p);
                D.Groups.RemoveAll(e => e.Persons.Contains(p));
            }
            else if (target.Name.StartsWith("kcs"))
            {
                var i = dropInfo.InsertIndex;
                var teacher = D.Teachers[int.Parse((string)target.Tag)];
                if (p.Instrument == teacher.Instrument)
                {
                    p.TimeSlot = i;
                    if (p.Teacher != null)
                        D.Schedule[D.Teachers.IndexOf(p.Teacher)].Remove(p);
                    p.Teacher = teacher;
                }
                else if (p.IsVocalistToo && teacher.Instrument == Instrument.Voice)
                {
                    p.VocalTimeSlot = i;
                    if (p.VocalTeacher != null)
                        D.Schedule[D.Teachers.IndexOf(p.VocalTeacher)].Remove(p);
                    p.VocalTeacher = teacher;
                }
                else return;

                var j = D.Students.IndexOf(p);
                defaultDropHandler.Drop(dropInfo); // This locates the correct ObservableCollection and inserts p, but sometimes also removes it from Students
                if (!D.Students.Contains(p))
                    D.Students.Insert(j, p);

            }
            else if (source.Name.StartsWith("kcs") && target.Name == "nokcs")
            {
                int i = int.Parse((string)source.Tag);

                Person q = D.Teachers[i];
                if (p.Teacher == q)
                    p.Teacher = null;
                else if (p.VocalTeacher == q)
                    p.VocalTeacher = null;

                D.Schedule[i].Remove(p);
            }
        }
    }
}
