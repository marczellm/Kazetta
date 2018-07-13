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
            var schedule = Teachers.Select(t => Students.Where(p => p.Teacher == t || p.VocalTeacher == t));
            Schedule = schedule.Select(seq => new ObservableCollection2<Person>(seq.ToArray())).ToList();

            scheduleInited = true;
        }

        public ICollectionView Fiuk => CollectionViewHelper.Lazy(Students, p => ((Person)p).Sex == Sex.Male);
        public ICollectionView Lanyok => CollectionViewHelper.Lazy(Students, p => ((Person)p).Sex == Sex.Female);
        public ICollectionView CsoportokbaOsztando => CollectionViewHelper.Lazy(Students, p => ((Person)p).Type == PersonType.Student
                                                                                                && ((((Person)p).Instrument == Instrument.Guitar && ((Person)p).Level != Level.Advanced)
                                                                                                   || ((Person)p).Instrument == Instrument.Solo));
        public ICollectionView Unscheduled => CollectionViewHelper.Lazy(Students,
            p => ((Person)p).Type == PersonType.Student && (
                 ((Person)p).Teacher == null || 
                 ((Person)p).TimeSlot < 0 ||
                 (((Person)p).IsVocalistToo && ((Person)p).VocalTeacher == null)
            ),
            new SortDescription("Name", ListSortDirection.Ascending));

        private ObservableCollection2<Group> groups;
        public ObservableCollection2<Group> Groups
        {
            get { return groups ?? (groups = new ObservableCollection2<Group>()); }
            private set { groups = value; RaisePropertyChanged(); }
        }
        private Group newGroup;
        public Group NewGroup
        {
            get { return newGroup ?? (newGroup = new Group()); }
            set { newGroup = value; RaisePropertyChanged(); }
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
                    Groups = Groups.ToArray()
                };
            }
            set
            {
                Students.AddRange(value.Students);
                Teachers.QuietClear();                
                Teachers.AddRange(value.Teachers);
                Groups.AddRange(value.Groups);
                // The XML serializer doesn't handle object references, so we replace Person copies with references
                foreach (Person student in Students)
                {
                    if (student.Teacher != null)
                        student.Teacher = Teachers.Single(p => p.Name == student.Teacher.Name);
                    if (student.VocalTeacher != null)
                        student.VocalTeacher = Teachers.Single(p => p.Name == student.VocalTeacher.Name);
                }
                foreach (Group group in Groups)
                    for (int i = 0; i < group.Persons.Count(); i++)
                        group.Persons[i] = Students.Single(p => p.Name == group.Persons[i].Name);
            }
        }
    }
}