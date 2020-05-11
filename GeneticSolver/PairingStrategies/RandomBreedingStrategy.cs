using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;

namespace GeneticSolver.PairingStrategies
{
    public class RandomBreedingStrategy : IPairingStrategy
    {
        private readonly System.Random _random = new System.Random();
        public IEnumerable<Tuple<T,T>> GetPairs<T>(IEnumerable<T> genomes)
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