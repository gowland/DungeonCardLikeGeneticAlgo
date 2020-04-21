using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;
using GeneticSolver.RequiredInterfaces;

namespace GeneticAlgo.Values
{
    public class MyThingSolverLogger : ISolverLogger<MyThing, int>
    {
        public void Start(ISolverParameters parameters)
        {
            Console.WriteLine("================ Start ================ ");
        }

        public void LogStartGeneration(int generationNumber)
        {
            Console.WriteLine($"------------- generation {generationNumber} -------------");
        }

        public void LogGenome(FitnessResult<MyThing, int> result)
        {
            var thing = result.GenomeInfo.Genome;
            Console.WriteLine($" {result.Fitness} - {thing.Sum} = {thing.A} + {thing.B} + {thing.C} + {thing.D} + {thing.E}");
        }

        public void End()
        {
            Console.WriteLine("================ Done ================ ");
        }

        public void LogGenerationInfo(GenerationResult<MyThing, int> generationResult)
        {
            Console.WriteLine($" Average generation: {generationResult.AverageGenomeGeneration}");
            Console.WriteLine($" Average fitness: {generationResult.OrderedGenomes.Average(r => r.Fitness)}");
        }
    }
}