using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenerationFactory : IGenerationFactory<Values>
    {
        public Values GetNewGenome()
        {
            return new Values();
        }
    }

    public class ValuesSolverLogger : ISolverLogger<Values, int>
    {
        public void LogStartGeneration(int generationNumber)
        {
            Console.WriteLine($"Starting generation {generationNumber}");
        }

        public void LogGenerationInfo(IEnumerable<FitnessResult<Values, int>> results)
        {
            Console.WriteLine($" Average score: {results.Average(r => r.Fitness)}");
        }
    }
}