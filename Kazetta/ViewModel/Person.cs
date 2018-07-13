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

        private int vocalTimeSlot;
        public int VocalTimeSlot
        {
            get { return vocalTimeSlot; }
            set { vocalTimeSlot = value; RaisePropertyChanged(); }
        }

        private Person vocalTeacher;
        public Person VocalTeacher
        {
            get { return vocalTeacher; }
            set { vocalTeacher = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
