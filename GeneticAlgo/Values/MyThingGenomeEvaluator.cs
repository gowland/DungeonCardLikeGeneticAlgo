using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class MyThingGenomeEvaluator : IGenomeEvaluator<MyThing, int>
    {
        public IEnumerable<FitnessResult<MyThing, int>> GetFitnessResults(IEnumerable<IGenomeInfo<MyThing>> genomes)
        {
            return genomes.Select(g => new FitnessResult<MyThing, int>(g, GetFitness(g.Genome)));
        }

        public IOrderedEnumerable<FitnessResult<MyThing, int>> SortDescending(IEnumerable<FitnessResult<MyThing, int>> genomes)
        {
            return genomes.OrderByDescending(r => r.Fitness);
        }

        private int GetFitness(MyThing thing)
        {
            int sum = thing.Sum;

            return IsPrime(sum) ? sum : sum / 4;
        }

        private static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i+=2)
                if (number % i == 0)
                    return false;

            return true;
        }
    }
}