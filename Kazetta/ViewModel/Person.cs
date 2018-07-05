using Kazetta.ViewModel;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Kazetta
{
    public enum Instrument { Guitar, Keyboards, Bass, Percussion, Solo, Voice }
    public enum Level { Beginner, Intermediate, Advanced }
    public enum Sex { Male, Female }
    public enum PersonType { Teacher, Student }

    [Serializable]
    public class Person : ViewModelBase
    {
        public string Name { get; set; }
        private PersonType type;
        public PersonType Type
        {
            get { return type; }
            set { type = value; }
        }
        private Instrument instrument;
        public Instrument Instrument
        {
            get { return instrument; }
            set { instrument = value; RaisePropertyChanged(); }
        }

        private bool pinned = false;
        public bool Pinned
        {
            get { return pinned; }
            set
            {
                pinned = value;
                RaisePropertyChanged();
                foreach (Person p in kivelIgen)
                    if (p.Pinned != value)
                        p.Pinned = value;
            }
        }


        private int birthYear = DateTime.Now.Year;
        public int BirthYear
        {
            get { return birthYear; }
            set { birthYear = value; RaisePropertyChanged(); RaisePropertyChanged("Age"); }
        }
        [XmlIgnore]
        public int Age
        {
            get { return DateTime.Now.Year - BirthYear; }
            set { BirthYear = DateTime.Now.Year - value; RaisePropertyChanged(); RaisePropertyChanged("BirthYear"); }
        }
        private Sex sex;
        public Sex Sex
        {
            get { return sex; }
            set { sex = value; RaisePropertyChanged(); }
        }        
        private Level level;
        public Level Level
        {
            get { return level; }
            set { level = value; RaisePropertyChanged(); }
        }
        public bool IsVocalistToo { get; set; }
        public bool IsVocalist => IsVocalistToo || Instrument == Instrument.Voice;
        
        public string BandName { get; set; }

        private int timeSlot;
        public int TimeSlot
        {
            get { return timeSlot; }
            set { timeSlot = value; RaisePropertyChanged(); }
        }

        private Person teacher;
        public Person Teacher
        {
            get { return teacher; }
            set { teacher = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// These will be filled out by <see cref="Algorithms.ConvertEdges"/> 
        /// </summary>
        internal HashSet<Person> kivelIgen = new HashSet<Person>(), kivelNem = new HashSet<Person>();

        /// <summary>
        /// Traverse the graphs defined by kivelIgen and kivelNem.
        /// Collect the transitively related nodes into these sets so that no further recursive traversal is needed during the algorithm.
        /// </summary>
        internal void CollectRecursiveEdges()
        {
            HashSet<Person> visitedSet = new HashSet<Person>();
            Queue<Person> queue = new Queue<Person>();
            foreach (Person p in kivelIgen)
                queue.Enqueue(p);
            while (queue.Count > 0)
            {
                Person p = queue.Dequeue();
                kivelIgen.Add(p);
                visitedSet.Add(p);
                foreach (Person q in p.kivelIgen)
                    if (!visitedSet.Contains(q))
                        queue.Enqueue(q);
                foreach (Person q in p.kivelNem)
                    kivelNem.Add(q);
            }
        }
    }
}
