using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;

namespace GeneticSolver.BreedingStrategies
{
    public class RandomBreedingStrategy : IPairingStrategy
    {
        private readonly Random _random = new Random();
        public IEnumerable<Tuple<T,T>> GetPairs<T>(IOrderedEnumerable<T> genomes)
        {
            var genomesArr = genomes.OrderBy(g => _random.NextDouble()).ToArray();

            while (genomesArr.Length > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<T, T>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }
    }
}