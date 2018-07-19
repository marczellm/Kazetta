using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Data;

namespace Kazetta
{
    public class Algorithms
    {
        private ViewModel.MainWindow d;
        private readonly List<Group> Beosztando;
        private static Random rng = new Random();
        private Object _lock = new Object();
        public Algorithms(ViewModel.MainWindow data)
        {
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
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
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
            Console.WriteLine(ct?.IsCancellationRequested);
            bool kesz = false;
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                Shuffle(Beosztando);

                // Put advanced guitarists first.

                bool isAdvancedGuitarist(Group g) {
                    Person p = g.Persons[0];
                    return p.Instrument == Instrument.Guitar && p.SkillLevel == Level.Advanced;
                };

                foreach (Group g in Beosztando.Where(g => isAdvancedGuitarist(g)))
                {
                    Beosztando.Remove(g);
                    Beosztando.Insert(0, g);
                }

                foreach (Group g in Beosztando)
                {
                    Person p = g.Persons[0];

                    if (g.Persons.Length > 1 || p.Pair == null) // we have to assign to an instrument teacher
                    {
                        var options = from i in Enumerable.Range(0, d.Teachers.Count)
                                      from j in Enumerable.Range(0, 7)
                                      where d.Teachers[i].Instrument == p.Instrument && d.CanAssign(g, i, j)
                                      select (i, j);

                        if (isAdvancedGuitarist(g))
                            options = options.Where(tup => d.Teachers[tup.i].Name == "Gyárfás István");

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

                    if (g.Persons.Length == 1 && p.IsVocalistToo) // we have to assign to a vocal teacher
                    {
                        var options = from i in Enumerable.Range(0, d.Teachers.Count)
                                      from j in Enumerable.Range(0, 7)
                                      where d.Teachers[i].IsVocalist && d.CanAssign(g, i, j)
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
                }
            }
            return kesz;
        }
    }
}