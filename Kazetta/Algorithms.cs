using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kazetta
{
    public class Algorithms
    {
        private ViewModel.MainWindow d;
        private readonly List<Group> Beosztando;
        private static Random rng = new Random();
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            Beosztando = d.Groups.ToList();
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
            d.ClearSchedule();

            bool kesz = false;
            Console.WriteLine(Beosztando.Count);
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                //Shuffle(Beosztando);
                foreach (Group g in Beosztando)
                {
                    Console.WriteLine(g);
                    var options = from i in Enumerable.Range(0, d.Teachers.Count)
                                  from j in Enumerable.Range(0, 7)
                                  where d.CanAssign(g, i, j)
                                  select (i, j);

                    if (options.Any())
                    {
                        var (i, j) = options.First();
                        d.AssignTo(g, i, j);
                    }
                    else
                    {
                        d.ClearSchedule();
                        kesz = false;
                    }                    
                }
            }
            return kesz;
        }
    }
}