namespace GeneticSolver
{
    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxGenerationSize, bool randomizeMating, bool mutateParents, double propertyMutationProbability)
        {
            MaxGenerationSize = maxGenerationSize;
            RandomizeMating = randomizeMating;
            MutateParents = mutateParents;
            PropertyMutationProbability = propertyMutationProbability;
        }

        public int MaxGenerationSize { get; }
        public bool RandomizeMating { get; }
        public bool MutateParents { get; }
        public double PropertyMutationProbability { get; }
    }
}