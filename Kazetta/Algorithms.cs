using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace Kazetta
{
    public class Algorithms
    {
        private readonly ViewModel.MainWindow d;
        private readonly List<Group> Beosztando;
        private static readonly Random rng = new Random();
        private readonly object _lock;
        public Algorithms(ViewModel.MainWindow data, object _lock)
        {
            this._lock = _lock;
            d = data;
            Beosztando = d.Groups.ToList();

            foreach (var schedule in d.Schedule)
                BindingOperations.EnableCollectionSynchronization(schedule, _lock);
        }        

        /// <summary>
        /// Randomly shuffles a list using the Fisher-Yates shuffle.
        /// </summary>
        private static IList<T> Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
            return list;
        }

        private static int SpecialIndexOf<T>(T[] arr, T item)
        {
            var ret = Array.IndexOf(arr, item);
            if (ret == -1)
                ret = arr.Length;
            return ret;
        }

        private static int SexBasedTeacherPreference(Person student, Person teacher)
        {
            return student.Sex == teacher.Sex ? 0 : 1;
        }

        /// <summary>
        /// Runs a naive first fit algorithm to determine a proper "graph coloring".
        /// The success of such an algorithm depends only on the given ordering of nodes.
        /// This implementation randomly shuffles the nodes when backtracking.
        /// </summary>
        /// <returns>whether the algorithm was successful</returns>
        public bool NaiveFirstFit(CancellationToken? ct = null)
        {
            lock (_lock)
                d.ClearSchedule();
            bool kesz = false;
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                Shuffle(Beosztando);

                // Put advanced guitarists first.
                bool isAdvancedGuitarist(Group g) {
                    Student p = g.Persons[0];
                    return p.Instrument == Instrument.Guitar && p.SkillLevel == Level.Advanced;
                };

                if (d.AdvancedGuitarists)
                {
                    foreach (Group g in Beosztando.Where(g => isAdvancedGuitarist(g)).ToList())
                    {
                        Beosztando.Remove(g);
                        Beosztando.Insert(0, g);
                    }
                }

                // Put people with vocal teacher preferences first.
                foreach (Group g in Beosztando.Where(g => g.Persons.Length == 1 && g.Persons[0].PreferredVocalTeachers.Any(x => x != null)).ToList())
                {
                    Beosztando.Remove(g);
                    Beosztando.Insert(0, g);
                }

                foreach (Group g in Beosztando)
                {
                    Student p = g.Persons[0];

                    if (g.Persons.Length == 1 && p.IsVocalistToo) // we have to assign to a vocal teacher
                    {
                        var options = from i in Enumerable.Range(0, d.Teachers.Count)
                                      from j in Enumerable.Range(0, 7)
                                      where d.Teachers[i].IsVocalist && d.CanAssign(g, i, j)
                                      orderby SpecialIndexOf(p.PreferredVocalTeachers, d.Teachers[i])
                                      orderby SexBasedTeacherPreference(p, d.Teachers[i])
                                      select (i, j);

                        if (options.Any())
                        {
                            var (i, j) = options.First();
                            lock (_lock)
                                d.AssignTo(g, i, j);
                        }
                        else
                        {
                            lock (_lock)
                                d.ClearSchedule();
                            kesz = false;
                            break;
                        }
                    }

                    if (g.Persons.Length > 1 || p.Pair == null) // we have to assign to an instrument teacher
                    {
                        var options = from i in Enumerable.Range(0, d.Teachers.Count)
                                      from j in Enumerable.Range(0, 7)
                                      where d.Teachers[i].Instruments.Contains(p.Instrument) && d.CanAssign(g, i, j)
                                      select (i, j);

                        if (p.Instrument == Instrument.Voice)
                            options = options.OrderBy(tup => SpecialIndexOf(p.PreferredVocalTeachers, d.Teachers[tup.i])).OrderBy(tup => SexBasedTeacherPreference(p, d.Teachers[tup.i]));

                        if (d.AdvancedGuitarists && isAdvancedGuitarist(g))
                            options = options.Where(tup => d.Teachers[tup.i].Name == "Gyarmati Fanny");

                        if (g.Persons.Any(q => q.VocalTeacher?.Name == "Szinnyai Dóri")) // They have to be free in the first 2 timeslots
                            options = options.Where(tup => tup.j > 1);

                        if (g.Persons[0].Name == "Hernády Balázs")
                            options = options.Where(tup => d.Teachers[tup.i].Name == "Fodor Tamás");

                        if (options.Any())
                        {
                            var (i, j) = options.First();
                            lock (_lock)
                                d.AssignTo(g, i, j);
                        }
                        else
                        {
                            lock (_lock)
                                d.ClearSchedule();
                            kesz = false;
                            break;
                        }
                    }                    
                }
            }
            if (kesz)
			{
                ExtraPostprocess();
			}
            return kesz;
        }

        private void ExtraPostprocess()
		{
            var solfeggioTeacherIndex = d.Teachers.IndexOf(d.Teachers.Single(t => t.Name == "Bartal Ági"));
            var improvTeacherIndex = d.Teachers.IndexOf(d.Teachers.Single(t => t.Name == "Vadász Gellért"));
            foreach (Group g in d.Schedule[solfeggioTeacherIndex])
			{
                if (g.Persons.Any())
                {
                    var nextTimeSlot = (g.Persons[0].TimeSlot + 1) % 7;
                    Debug.Assert(!g.Persons.Any(p => p.VocalTimeSlot == nextTimeSlot));
                    d.Schedule[improvTeacherIndex][(g.Persons[0].TimeSlot + 1) % 7] = g;
                }
			}
        }
    }
}