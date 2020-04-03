using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenomeEvaluator : IGenomeEvalautor<Values>
    {
        public int GetFitness(IGenome<Values> genome)
        {
            int sum = genome.Value.Sum;

            return IsPrime(sum) ? sum : sum / 4;
        }

        public IEnumerable<FitnessResult<Values>> GetFitnessResults(IEnumerable<IGenome<Values>> genomes)
        {
            return genomes.Select(g => new FitnessResult<Values>(g, GetFitness(g)));
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