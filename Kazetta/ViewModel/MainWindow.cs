using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System;
using System.Diagnostics;

namespace Kazetta.ViewModel
{
    /// <summary>
    /// Because this is not an enterprise app, I didn't create the plumbing necessary to have separate ViewModels for each tab.
    /// Instead I dumped all of the application state in the below class.
    /// </summary>
    public class MainWindow: ViewModelBase
    {
        /// <summary>
        /// Most tabs disable if this is false
        /// </summary>
        public bool PeopleNotEmpty => People.Count() != 0;

        /// <summary>
        /// The Save button disables if this is false
        /// </summary>
        public bool BeosztasKesz => !Band(-1).Any() && !Room(-1).Any();

        private bool magicAllowed = false;
        private bool magicPossible = false;
        public bool MagicAllowed  { get { return magicAllowed; }  set { magicAllowed = value;  RaisePropertyChanged("MagicEnabled"); } }
        public bool MagicPossible { get { return magicPossible; } set { magicPossible = value; RaisePropertyChanged(); RaisePropertyChanged("MagicEnabled"); } }
        public bool MagicEnabled => MagicAllowed && MagicPossible;

        public static int WeekendNumber => 2 * DateTime.Now.Year - 4013 + DateTime.Now.Month / 7;

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");
            RaisePropertyChanged("Zeneteamvezeto");
            RaisePropertyChanged("Fiuvezeto");
            RaisePropertyChanged("Lanyvezeto");
            RaisePropertyChanged("Kiscsoportok");
        }
                
        private ObservableCollection2<Person> people;
        public ObservableCollection2<Person> People
        {
            get
            {
                if (people == null)
                {
                    people = new ObservableCollection2<Person>();
                    people.CollectionChanged += People_CollectionChanged;
                }
                return people;
            }
            private set
            {
                people = value;
                RaisePropertyChanged();
                RaisePropertyChanged("PeopleNotEmpty");
            }
        }
        private volatile bool kiscsoportInited = false, alvocsoportInited = false;

        /// <summary>
        /// This method is called when the kiscsoportbeoszto tab is opened and all conditions have been met.
        /// </summary>
        internal void InitKiscsoport()
        {
            if (kiscsoportInited)
                return;
            bands = Enumerable.Range(0, 15).Select(i => BandCollectionView(i)).ToList();
            
            NoBand.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            kiscsoportInited = true;
            RaisePropertyChanged("Kiscsoportok");
            RaisePropertyChanged("NoKiscsoport");
        }       

        /// <summary>
        /// This method is called when the alvocsoportbeoszto tab is successfully opened and all conditions have been met.
        /// </summary>
        internal void InitAlvocsoport()
        {
            if (alvocsoportInited)
                return;
            rooms = Enumerable.Range(0, 15).Select(i => RoomCollectionView(i)).ToList();
            
            NoRoom_Male.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");
            NoRoom_Female.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            alvocsoportInited = true;
            RaisePropertyChanged("Alvocsoportok");
            RaisePropertyChanged("NoAlvocsoport");
        }

        /// <summary>
        /// Reorder the sleeping groups so that the girl sleeping groups come first, and if possible, the boys begin in the second row
        /// </summary>
        /// <param name="displayRowLength">The number of groups that can be displayed in one row</param>
        public void AlvocsoportDisplayOrdering(int displayRowLength)
        {
            //int i = 0;
            //foreach (Person q in Alvocsoportvezetok.Where(p => p.Sex == Sex.Female).OrderBy(p => p.Name).ToList())
            //{
            //    int j = q.Room;
            //    SwapAlvocsoports(i, j);
            //    i++;
            //}
            //if (i <= displayRowLength && Alvocsoportvezetok.Count(p => p.Sex == Sex.Male) <= displayRowLength)            
            //    i = displayRowLength;
            //foreach (Person q in Alvocsoportvezetok.Where(p => p.Sex == Sex.Male).OrderBy(p => p.Name).ToList())
            //{
            //    int j = q.Room;
            //    SwapAlvocsoports(i, j);
            //    i++;
            //}
        }

        /// <summary>
        /// Reorder the sleeping groups so that they are numbered consecutively with no gaps between
        /// </summary>
        internal void AlvocsoportExportOrdering()
        {
            for (int i = 0; i < Rooms.Count() - 1; i++)
            {
                int j = Enumerable.Range(0, Rooms.Count()).Last(k => Room(k).Any());
                if (!Room(i).Any() && i < j)                    
                    SwapAlvocsoports(i, j);
            }
        }

        public ICollectionView Fiuk => CollectionViewHelper.Lazy(People, p => ((Person)p).Sex == Sex.Male);
        public ICollectionView Lanyok => CollectionViewHelper.Lazy(People, p => ((Person)p).Sex == Sex.Female);
        
        private ICollectionView BandCollectionView(int i)
        {
            var ret = CollectionViewHelper.Get(People, p => ((Person)p).Band == i);
            return ret;
        }
        private ICollectionView RoomCollectionView(int i)
        {
            var ret = CollectionViewHelper.Get(People, p => ((Person)p).Room == i);
            return ret;
        }
        public IEnumerable<Person> Band(int i)
        {
            return People.Where(p => p.Band == i);
        }
        public IEnumerable<Person> Room(int i)
        {
            return People.Where(p => p.Room == i);
        }
        public ICollectionView NoBand => CollectionViewHelper.Lazy(People, p => ((Person)p).Band == -1 );
        public ICollectionView NoRoom_Male => CollectionViewHelper.Lazy(People, p => ((Person)p).Room == -1 && ((Person)p).Sex == Sex.Male);
        public ICollectionView NoRoom_Female => CollectionViewHelper.Lazy(People, p => ((Person)p).Room == -1 && ((Person)p).Sex == Sex.Female);

        private List<ICollectionView> bands, rooms;
        public List<ICollectionView> Bands => bands;
        public List<ICollectionView> Rooms => rooms;        
        
        private ObservableCollection2<Edge> edges;
        public ObservableCollection2<Edge> Edges
        {
            get { return edges ?? (edges = new ObservableCollection2<Edge>()); }
            private set { edges = value; RaisePropertyChanged(); }
        }
        private Edge edge;
        public Edge Edge
        {
            get { return edge ?? (edge = new Edge()); }
            set { edge = value; RaisePropertyChanged(); }
        }
        private int maxAgeDifference = 8;
        public int MaxAgeDifference
        {
            get { return maxAgeDifference; }
            set { maxAgeDifference = value; RaisePropertyChanged(); }
        }
        public Algorithms Algorithm { get; set; }
        private string statusText = "";
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(); }
        }
        public string[] AlvocsoportNevek { get; } = new string[15];

        /// <summary>
        /// Represents groups in which no two persons should get assigned to the same sharing group.
        /// </summary>
        public ObservableCollection2<ObservableCollection2<Person>> MutuallyExclusiveGroups { get; } = new ObservableCollection2<ObservableCollection2<Person>> { new ObservableCollection2<Person>() };

        internal AppData AppData
        {
            get
            {
                return new AppData
                {
                    Persons = People.ToArray(),
                    Edges = Edges.ToArray(),
                    MutuallyExclusiveGroups = MutuallyExclusiveGroups.Select(g => g.ToArray()).ToArray(),
                    Szentendre = Szentendre.ToArray()
                };
            }
            set
            {
                People.AddRange(value.Persons);
                Edges.AddRange(value.Edges);
                // The XML serializer doesn't handle object references, so we replace Person copies with references
                foreach (Edge edge in Edges)
                    for (int i = 0; i < edge.Persons.Count(); i++)
                        edge.Persons[i] = People.Single(p => p.Name == edge.Persons[i].Name);                
                foreach (var group in value.MutuallyExclusiveGroups)
                {
                    var og = new ViewModel.ObservableCollection2<Person>();
                    og.AddRange(group.Select(p => People.Single(q => q.Name == p.Name)));
                    MutuallyExclusiveGroups.Add(og);
                }
                MutuallyExclusiveGroups.RemoveAll(g => !g.Any());
                if (!MutuallyExclusiveGroups.Any())
                    MutuallyExclusiveGroups.Add(new ObservableCollection2<Person>());
                RaisePropertyChanged("MutuallyExclusiveGroups");

                if (WeekendNumber == 20)
                    Szentendre.AddRange(value.Szentendre.Select(p => People.Single(q => q.Name == p.Name)));
            }
        }

        public void SwapKiscsoports(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            foreach (Person p in Band(i).ToList())
                p.Band = -100;
            foreach (Person p in Band(j).ToList())
                p.Band = i;
            foreach (Person p in Band(-100).ToList())
                p.Band = j;
        }

        public void SwapAlvocsoports(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            foreach (Person p in Room(i).ToList())
                p.Room = -100;
            foreach (Person p in Room(j).ToList())
                p.Room = i;
            foreach (Person p in Room(-100).ToList())
                p.Room = j;

            string temp = AlvocsoportNevek[i];
            AlvocsoportNevek[i] = AlvocsoportNevek[j];
            AlvocsoportNevek[j] = temp;
        }

        #region Extras
        public ObservableCollection2<Person> Szentendre { get; } = new ObservableCollection2<Person>();
        #endregion
    }
}