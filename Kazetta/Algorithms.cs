using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kazetta
{
    public class Algorithms
    {
        private ViewModel.MainWindow d;
        private readonly List<Person> Beosztando;
        private static Random rng = new Random();
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            Beosztando = d.Students.Cast<Person>().ToList();
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
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                Shuffle(Beosztando);
                foreach (Person p in Beosztando)
                {
                    throw new NotImplementedException();
                    //if (!p.Kiscsoportvezeto)
                    //{
                    //    var options = Enumerable.Range(0, m).Where(i => !Conflicts(p, i));
                    //    if (options.Any())
                    //    {
                    //        if (p.Type == PersonType.Ujonc) // ha újonc, akkor próbáljuk olyan helyre tenni, ahol még kevés újonc van
                    //            AssignToKiscsoport(p, options.MinBy(i => d.Kiscsoport(i).Count(q => q.Type == PersonType.Ujonc)));
                    //        else // különben ahol kevés ember van
                    //            AssignToKiscsoport(p, options.MinBy(i => d.Kiscsoport(i).Count()));
                    //    }
                    //    else // Nincs olyan kiscsoport, ahova be lehetne tenni => elölről kezdjük
                    //    {
                    //        foreach (Person q in Beosztando)
                    //            if (!q.Kiscsoportvezeto)
                    //                q.Kiscsoport = -1;
                    //        foreach (Person q in Kiscsoportvezetok)
                    //            AssignToKiscsoport(q, q.Kiscsoport);
                    //        kesz = false;
                    //        break;
                    //    }
                    //}
                }
            }
            return kesz;
        }
    }
}