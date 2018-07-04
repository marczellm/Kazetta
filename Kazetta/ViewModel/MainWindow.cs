using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
        public bool PeopleNotEmpty => People.Any();

        private bool magicAllowed = false;
        private bool magicPossible = false;
        public bool MagicAllowed  { get { return magicAllowed; }  set { magicAllowed = value;  RaisePropertyChanged("MagicEnabled"); } }
        public bool MagicPossible { get { return magicPossible; } set { magicPossible = value; RaisePropertyChanged(); RaisePropertyChanged("MagicEnabled"); } }
        public bool MagicEnabled => MagicAllowed && MagicPossible;

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");        
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

        private volatile bool kiscsoportInited = false;

        /// <summary>
        /// This method is called when the kiscsoportbeoszto tab is opened and all conditions have been met.
        /// </summary>
        internal void InitKiscsoport()
        {
            if (kiscsoportInited)
                return;
            //bands = Enumerable.Range(0, 15).Select(i => BandCollectionView(i)).ToList();
            
            //NoBand.CollectionChanged += (s, e) => RaisePropertyChanged("BeosztasKesz");

            kiscsoportInited = true;
            RaisePropertyChanged("Kiscsoportok");
            RaisePropertyChanged("NoKiscsoport");
        }

        public ICollectionView Students => CollectionViewHelper.Lazy(People, p => ((Person)p).Type == PersonType.Student);
        public ICollectionView Teachers => CollectionViewHelper.Lazy(People, p => ((Person)p).Type == PersonType.Teacher, new SortDescription("Instrument", ListSortDirection.Ascending));
        public ICollectionView Fiuk => CollectionViewHelper.Lazy(People, p => ((Person)p).Sex == Sex.Male);
        public ICollectionView Lanyok => CollectionViewHelper.Lazy(People, p => ((Person)p).Sex == Sex.Female);
        public ICollectionView CsoportokbaOsztando => CollectionViewHelper.Lazy(People, p => ((Person)p).Type == PersonType.Student);

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
                    Edges = Edges.ToArray()                    
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
            }
        }

        public void SwapKiscsoports(int i, int j)
        {
            Debug.Assert(i != -100);
            Debug.Assert(j != -100);
            if (i == j) return;
            //foreach (Person p in Band(i).ToList())
            //    p.Band = -100;
            //foreach (Person p in Band(j).ToList())
            //    p.Band = i;
            //foreach (Person p in Band(-100).ToList())
            //    p.Band = j;
        }
    }
}