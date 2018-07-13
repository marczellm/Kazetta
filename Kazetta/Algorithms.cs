﻿using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kazetta
{
    public class Algorithms
    {
        private ViewModel.MainWindow d;
        private List<Person> Beosztando;
        private static Random rng = new Random();
        public Algorithms(ViewModel.MainWindow data)
        {
            d = data;
            Beosztando = d.Students.Cast<Person>().ToList();
        }

        private void AssignToKiscsoport(Person p, int kiscsoport)
        {
            throw new NotImplementedException();
        }

        public bool Conflicts(Person p, int kiscsoport)
        {
            throw new NotImplementedException();
        }

        public bool Conflicts(Person p, int kiscsoport, out string message)
        {
            throw new NotImplementedException();
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
        /// Generates all possible permutations of an enumerable
        /// </summary>
        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Runs a naive first fit algorithm to determine a proper "graph coloring".
        /// The success of such an algorithm depends only on the given ordering of nodes.
        /// This implementation randomly shuffles the nodes when backtracking.
        /// </summary>
        /// <returns>whether the algorithm was successful</returns>
        public bool NaiveFirstFit(CancellationToken? ct = null)
        {
            foreach (Person p in Beosztando)
                if (!p.Pinned)
                    ;// p.Band = -1; //TODO
            //Beosztando.RemoveAll((Person p) => p.Band != -1);

            bool kesz = false;
            while (!kesz && ct?.IsCancellationRequested != true) // generate random orderings of People and run the first-fit coloring until it is complete or cancelled
            {
                kesz = true;
                Shuffle(Beosztando);
                foreach (Person p in Beosztando)
                {
                    throw new NotImplementedException();
                }
            }
            return kesz;
        }
    }
}