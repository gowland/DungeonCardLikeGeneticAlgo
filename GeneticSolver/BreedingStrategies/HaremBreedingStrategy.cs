using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;

namespace GeneticSolver.BreedingStrategies
{
    public class HaremBreedingStrategy : IBreadingStrategy
    {
        public IEnumerable<Tuple<T, T>> GetPairs<T>(IOrderedEnumerable<T> genomes)
        {
            var genomesArr = genomes.ToArray();
            var king = genomesArr.First();
            return genomesArr.Skip(1).Select(concubine => new Tuple<T, T>(king, concubine));
        }
    }
}