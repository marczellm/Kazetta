using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Kazetta.ViewModel
{
    /// <summary>
    /// Because this is not an enterprise app, I didn't create the plumbing necessary to have separate ViewModels for each tab.
    /// Instead I dumped all of the application state in the below class.
    /// </summary>
    public class MainWindow : ViewModelBase
    {
        /// <summary>
        /// Most tabs disable if this is false
        /// </summary>
        public bool PeopleNotEmpty => Students.Any();

        private bool magicAllowed = false;
        private bool magicPossible = false;
        public bool MagicAllowed { get { return magicAllowed; } set { magicAllowed = value; RaisePropertyChanged("MagicEnabled"); } }
        public bool MagicPossible { get { return magicPossible; } set { magicPossible = value; RaisePropertyChanged(); RaisePropertyChanged("MagicEnabled"); } }
        public bool MagicEnabled => MagicAllowed && MagicPossible;

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PeopleNotEmpty");
        }

        private ObservableCollection2<Person> students;
        public ObservableCollection2<Person> Students
        {
            get
            {
                if (students == null)
                {
                    students = new ObservableCollection2<Person>();
                    students.CollectionChanged += People_CollectionChanged;
                }
                return students;
            }
            private set
            {
                students = value;
                RaisePropertyChanged();
                RaisePropertyChanged("PeopleNotEmpty");
            }
        }

        private ObservableCollection2<Person> teachers;
        public ObservableCollection2<Person> Teachers
        {
            get
            {
                if (teachers == null)
                {
                    teachers = new ObservableCollection2<Person>(new Person[16]);
                    teachers.CollectionChanged += People_CollectionChanged;
                }
                return teachers;
            }
            private set
            {
                teachers = value;
                RaisePropertyChanged();
            }
        }


        public List<ObservableCollection2<Person>> Schedule
        {
            get { return _schedule; }
            private set
            {
                _schedule = value;
                RaisePropertyChanged();
            }
        }

        private volatile bool scheduleInited = false;

        /// <summary>
        /// This method is called when the ScheduleTab tab is opened and all conditions have been met.
        /// </summary>
        internal void InitSchedule()
        {
            if (scheduleInited)
                return;
            var schedule = Teachers.Select(t => Students.Where(p => p.Teacher == t));
            Schedule = schedule.Select(seq => new ObservableCollection2<Person>(seq.ToArray())).ToList();

            scheduleInited = true;
        }

        public ICollectionView Fiuk => CollectionViewHelper.Lazy(Students, p => ((Person)p).Sex == Sex.Male);
        public ICollectionView Lanyok => CollectionViewHelper.Lazy(Students, p => ((Person)p).Sex == Sex.Female);
        public ICollectionView CsoportokbaOsztando => CollectionViewHelper.Lazy(Students, p => ((Person)p).Type == PersonType.Student);
        public ICollectionView Unscheduled => CollectionViewHelper.Lazy(Students,
            p => ((Person)p).Type == PersonType.Student && (((Person)p).Teacher == null || ((Person)p).TimeSlot < 0),
            new SortDescription("Name", ListSortDirection.Ascending));

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
        private List<ObservableCollection2<Person>> _schedule;

        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(); }
        }

        internal AppData AppData
        {
            get
            {
                return new AppData
                {
                    Students = Students.ToArray(),
                    Teachers = Teachers.ToArray(),
                    Edges = Edges.ToArray()
                };
            }
            set
            {
                Students.AddRange(value.Students);
                Teachers.QuietClear();                
                Teachers.AddRange(value.Teachers);
                Edges.AddRange(value.Edges);
                // The XML serializer doesn't handle object references, so we replace Person copies with references
                foreach (Person student in Students)
                    if (student.Teacher != null)
                        student.Teacher = Teachers.Single(p => p.Name == student.Teacher.Name);
                foreach (Edge edge in Edges)
                    for (int i = 0; i < edge.Persons.Count(); i++)
                        edge.Persons[i] = Students.Single(p => p.Name == edge.Persons[i].Name);
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