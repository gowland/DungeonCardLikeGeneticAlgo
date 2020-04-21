using System.Linq;

namespace GeneticSolver
{
    public class GenerationResult <T, TScore>
    {
        public GenerationResult(int generationNumber, IOrderedEnumerable<FitnessResult<T, TScore>> orderedGenomes)
        {
            GenerationNumber = generationNumber;
            OrderedGenomes = orderedGenomes;
            FittestGenome = orderedGenomes.First();
            AverageGenomeGeneration = orderedGenomes.Average(g => g.GenomeInfo.Generation);
        }

        public int GenerationNumber { get; }
        public IOrderedEnumerable<FitnessResult<T, TScore>> OrderedGenomes { get; }
        public FitnessResult<T, TScore> FittestGenome { get; }
        public double AverageGenomeGeneration { get; }
    }
}