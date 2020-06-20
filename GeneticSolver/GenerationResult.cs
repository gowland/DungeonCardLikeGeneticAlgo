using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public interface IGenerationResult<T, TScore>
    {
        int GenerationNumber { get; }
        IEnumerable<FitnessResult<T, TScore>> OrderedGenomes { get; }
        FitnessResult<T, TScore> FittestGenome { get; }
        double AverageGenomeGeneration { get; }
    }

    public class GenerationResult <T, TScore> : IGenerationResult<T, TScore>
        where TScore : IComparable<TScore>
    {
        public GenerationResult(int generationNumber, ScoredGeneration<T, TScore> orderedGenomes)
        {
            GenerationNumber = generationNumber;
            OrderedGenomes = orderedGenomes.OrderedFitnessResults.ToArray();
            FittestGenome = OrderedGenomes.First();
            AverageGenomeGeneration = OrderedGenomes.Average(g => g.GenomeInfo.Generation);
        }

        public int GenerationNumber { get; }
        public IEnumerable<FitnessResult<T, TScore>> OrderedGenomes { get; }
        public FitnessResult<T, TScore> FittestGenome { get; }
        public double AverageGenomeGeneration { get; }
    }
}