using System;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Linq;
using System.Collections.ObjectModel;
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
            dropInfo.DropTargetAdorner = null;
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            if (!(dropInfo.Data is Person))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            var p = (Person)dropInfo.Data;
            if (p.Sex == Sex.Male && target.Name == "Lanyvezeto"
             || p.Sex == Sex.Female && target.Name == "Fiuvezeto"
             || source.Name == "PeopleView" && target.Name != "PeopleView" && target.Name != "AddOrRemovePersonButton")
            {
                dropInfo.Effects = DragDropEffects.None;
            }
            else if (p.Pinned)
            {
                dropInfo.Effects = DragDropEffects.None;
                D.StatusText = p + " le van rögzítve!";
            }            
            else if (target.Name.StartsWith("kcs"))
            {
                int kcsn = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                string message = null;
                dropInfo.Effects = (kcsn != p.Band && D.Algorithm.Conflicts(p, kcsn, out message)) ? DragDropEffects.None : DragDropEffects.Move;
                D.StatusText = message;
            }
            else if (target.Name.StartsWith("acs"))
            {
                int acsn = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                var acsvez = D.Rooms[acsn].Cast<Person>().First();
                dropInfo.Effects = (p.Sex != acsvez.Sex) ? DragDropEffects.None : DragDropEffects.Move;
            }
            else
                dropInfo.Effects = DragDropEffects.Move;
        }        

        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var target = (FrameworkElement)dropInfo.VisualTarget;
            var source = (FrameworkElement)dropInfo.DragInfo.VisualSource;
            Person p = (Person)dropInfo.Data;
            switch (target.Name)
            {
                case "Fiuk": p.Sex = Sex.Male; break;
                case "Lanyok": p.Sex = Sex.Female; break;
                case "AddOrRemovePersonButton":
                    D.People.Remove(p);
                    D.Edges.RemoveAll(e => e.Persons.Contains(p));
                    break;  
            }            
            if (target.Name.StartsWith("kcs"))
                p.Band = Int32.Parse(target.Name.Remove(0, 3)) - 1;
            if (target.Name.StartsWith("acs"))
            {
                p.Room = Int32.Parse(target.Name.Remove(0, 3)) - 1;
                ((ItemsControl)source).Items.Refresh(); // This updates the visualizing decorations for all others in the source
                ((ItemsControl)target).Items.Refresh(); // and target sleeping groups
            }
            if (target.Name == "nokcs")
                p.Band = -1;
            if (target.Name.StartsWith("noacs"))
                p.Room = -1;
        }        
    }
}
