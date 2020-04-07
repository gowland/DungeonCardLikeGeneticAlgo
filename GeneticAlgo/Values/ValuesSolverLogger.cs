using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesSolverLogger : ISolverLogger<Values, int>
    {
        public void LogStartGeneration(int generationNumber)
        {
            Console.WriteLine($"Starting generation {generationNumber}");
        }

        public void LogGenerationInfo(IEnumerable<FitnessResult<Values, int>> results)
        {
            Console.WriteLine($" Average age: {results.Average(r => r.Genome.Generation)}");
            Console.WriteLine($" Average score: {results.Average(r => r.Fitness)}");
        }

        public void LogGenome(FitnessResult<Values, int> result)
        {
            var values = result.Genome.Genome;
            Console.WriteLine($" {result.Fitness} - {values.Sum} = {values.A} + {values.B} + {values.C} + {values.D} + {values.E}");
        }
    }
}