using Kazetta.ViewModel;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace Kazetta
{
	public enum Instrument { Guitar, Keyboards, Bass, Percussion, Solo, Voice }
	public enum Level { Beginner, Intermediate, Advanced }
	public enum Sex { Male, Female }

	[Serializable]
	public class Person : ViewModelBase
	{
		public string Name { get; set; }

		private bool pinned = false;
		public bool Pinned
		{
			get => pinned;
			set { pinned = value; RaisePropertyChanged(); }
		}

		private int birthYear = DateTime.Now.Year;
		public int BirthYear
		{
			get => birthYear;
			set { birthYear = value; RaisePropertyChanged(); RaisePropertyChanged("Age"); }
		}
		[XmlIgnore]
		public int Age
		{
			get => DateTime.Now.Year - BirthYear;
			set { BirthYear = DateTime.Now.Year - value; RaisePropertyChanged(); RaisePropertyChanged("BirthYear"); }
		}
		private Sex sex;
		public Sex Sex
		{
			get => sex;
			set { sex = value; RaisePropertyChanged(); }
		}

		public override string ToString() => Name;
	}

	[Serializable]
	public class Student : Person
	{
		private Instrument instrument;
		public Instrument Instrument
		{
			get => instrument;
			set { instrument = value; RaisePropertyChanged(); }
		}
		public bool IsVocalistToo { get; set; }
		public bool IsVocalist => IsVocalistToo || Instrument == Instrument.Voice;
		private Level level;
		public Level SkillLevel
		{
			get => level;
			set { level = value; RaisePropertyChanged(); }
		}
		private int timeSlot = -1;
		public int TimeSlot
		{
			get => timeSlot;
			set { timeSlot = value; RaisePropertyChanged(); }
		}

		private Teacher teacher;
		public Teacher Teacher
		{
			get => teacher;
			set { teacher = value; RaisePropertyChanged(); }
		}

		private int vocalTimeSlot = -1;
		public int VocalTimeSlot
		{
			get => vocalTimeSlot;
			set { vocalTimeSlot = value; RaisePropertyChanged(); }
		}
		private Teacher vocalTeacher;
		public Teacher VocalTeacher
		{
			get => vocalTeacher;
			set { vocalTeacher = value; RaisePropertyChanged(); }
		}

		private Student _pair;
		[XmlIgnore]
		public Student Pair
		{
			get => _pair;
			set { _pair = value; RaisePropertyChanged(); }
		}

		public Teacher[] PreferredVocalTeachers = new Teacher[2];

	}

	[Serializable]
	public class Teacher : Person
	{
		private Instrument[] instruments;
		public Instrument[] Instruments
		{
			get => instruments;
			set { instruments = value; RaisePropertyChanged(); }
		}

		public bool IsVocalist => Instruments.Contains(Instrument.Voice);
	}
}
