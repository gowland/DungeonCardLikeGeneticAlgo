using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;

namespace GeneticSolver.BreedingStrategies
{
    public class StratifiedBreedingStrategy : IPairingStrategy
    {
        public IEnumerable<Tuple<T,T>> GetPairs<T>(IEnumerable<T> genomes)
        {
            var genomesArr = genomes.ToArray();

            while (genomesArr.Length > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<T,T>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }
    }
}