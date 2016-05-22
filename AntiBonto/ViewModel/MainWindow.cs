﻿using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows;

namespace AntiBonto.ViewModel
{
    /// <summary>
    /// ObservableCollection with added AddRange support
    /// </summary>
    class ObservableCollection2<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection)
            {
                Items.Add(i);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    class MainWindow: ViewModelBase, IDropTarget
    {
        private ObservableCollection2<Person> ocp = new ObservableCollection2<Person>();
        /// <summary>
        /// Some UI elements disable based on this
        /// </summary>
        public bool PeopleNotEmpty
        {
            get { return People.Count() != 0; }
        }
        public MainWindow()
        {
            ocp.CollectionChanged += Ocp_CollectionChanged;            
        }
        /// <summary>
        /// So we need to keep them up to date
        /// </summary>
        private void Ocp_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");
            RaisePropertyChanged("Zeneteamvezeto");
            RaisePropertyChanged("Fiuvezeto");
            RaisePropertyChanged("Lanyvezeto");
        }
        /// <summary>
        /// Set where drops are allowed
        /// </summary>
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = null;
            var hova = (FrameworkElement) dropInfo.VisualTarget;
            var kit = (Person)dropInfo.Data;
            if (kit.Nem == Nem.Fiu && hova.Name == "Lanyvezeto" || kit.Nem == Nem.Lany && hova.Name == "Fiuvezeto")
                dropInfo.Effects = DragDropEffects.None;
            else
                dropInfo.Effects = DragDropEffects.Move;
        }
        /// <summary>
        /// Make the necessary data changes upon drop
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {
            var kik = (FrameworkElement) dropInfo.VisualTarget;
            Person p = (Person)dropInfo.Data;
            switch(kik.Name)
            {
                case "Fiuk": p.Nem = Nem.Fiu; break;
                case "Lanyok": p.Nem = Nem.Lany; break;
                case "Nullnemuek": p.Nem = null; break;
                case "Team": p.Type = PersonType.Teamtag; break;
                case "Zeneteam": p.Type = PersonType.Zeneteamtag; break;
                case "Ujoncok": p.Type = PersonType.Ujonc; break;
                case "Zeneteamvezeto": Zeneteamvezeto = p; break;
                case "Lanyvezeto": Lanyvezeto = p; break;
                case "Fiuvezeto": Fiuvezeto = p; break;
                case "Kiscsoportvezetok": p.Kiscsoportvezeto = true; break;                
            }
            if (((FrameworkElement)dropInfo.DragInfo.VisualSource).Name == "Kiscsoportvezetok" && (kik.Name == "Team" || kik.Name == "Ujoncok"))
                p.Kiscsoportvezeto = false;
        }

        public ObservableCollection2<Person> People
        {
            get
            {
                return ocp;
            }
            private set
            {
                ocp = value;
                RaisePropertyChanged();
                RaisePropertyChanged("PeopleNotEmpty");
            }
        }
       public ICollectionView Fiuk
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Nem" } };
                cvs.View.Filter = p => ((Person)p).Nem == Nem.Fiu;
                return cvs.View;
            }
        }
        public ICollectionView Lanyok
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Nem" } };
                cvs.View.Filter = p => ((Person)p).Nem == Nem.Lany;
                return cvs.View;
            }
        }
        public ICollectionView Nullnemuek
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Nem" } };
                cvs.View.Filter = p => ((Person)p).Nem == null;
                return cvs.View;
            }
        }
        public ICollectionView Ujoncok
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                cvs.View.Filter = p => ((Person)p).Type == PersonType.Ujonc;
                return cvs.View;
            }
        }
        public ICollectionView Team
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };                
                cvs.View.Filter = p => ((Person)p).Type != PersonType.Egyeb && ((Person)p).Type != PersonType.Ujonc;
                return cvs.View;
            }
        }
        public ICollectionView Kiscsoportvezetok
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Kiscsoportvezeto" } };
                cvs.View.Filter = p => ((Person)p).Kiscsoportvezeto;
                return cvs.View;
            }
        }
        public ICollectionView Zeneteam
        {
            get
            {
                CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Type" } };
                cvs.View.Filter = p => ((Person)p).Type == PersonType.Zeneteamtag;
                return cvs.View;
            }
        }
        public Person Zeneteamvezeto
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.Zeneteamvezeto);
            }
            set
            {
                if (Zeneteamvezeto != null)
                    Zeneteamvezeto.Type = PersonType.Teamtag;
                value.Type = PersonType.Zeneteamvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged("Fiuvezeto");
                RaisePropertyChanged("Lanyvezeto");
            }
        }
        public Person Fiuvezeto
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.Fiuvezeto);
            }
            set
            {
                if (Fiuvezeto != null)
                    Fiuvezeto.Type = PersonType.Teamtag;
                value.Type = PersonType.Fiuvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged("Zeneteamvezeto");
            }
        }
        public Person Lanyvezeto
        {
            get
            {
                return People.SingleOrDefault(p => p.Type == PersonType.Lanyvezeto);
            }
            set
            {
                if (Lanyvezeto != null)
                    Lanyvezeto.Type = PersonType.Teamtag;
                value.Type = PersonType.Lanyvezeto;
                RaisePropertyChanged();
                RaisePropertyChanged("Zeneteamvezeto");
            }
        }
        public ICollectionView Kiscsoport(int i)
        {
            CollectionViewSource cvs = new CollectionViewSource { Source = People, IsLiveFilteringRequested = true, LiveFilteringProperties = { "Kiscsoport" } };
            cvs.View.Filter = p => ((Person)p).Kiscsoport == i;
            return cvs.View;
        }
        public ICollectionView[] Kiscsoportok
        {
            get { return Enumerable.Range(0, Kiscsoportvezetok.OfType<Person>().Count()).Select(i => Kiscsoport(i)).ToArray(); }
        }
        private ObservableCollection<Edge> edges = new ObservableCollection<Edge>();
        public ObservableCollection<Edge> Edges
        {
            get { return edges; }
            private set { edges = value; RaisePropertyChanged(); }
        }
        private Edge edge;
        public Edge Edge
        {
            get { return edge ?? (edge = new Edge()); }
            set { edge = value; RaisePropertyChanged(); }
        }
    }
}