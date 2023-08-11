using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxEliteSize, int initialGenerationSize,
            double propertyMutationProbability)
        {
            MaxEliteSize = maxEliteSize;
            PropertyMutationProbability = propertyMutationProbability;
            InitialGenerationSize = initialGenerationSize;
        }

        public int MaxEliteSize { get; }
        public int InitialGenerationSize { get; }
        public double PropertyMutationProbability { get; }
    }
}