namespace GeneticSolver
{
    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxGenerationSize, bool randomizeMating, bool mutateParents)
        {
            MaxGenerationSize = maxGenerationSize;
            RandomizeMating = randomizeMating;
            MutateParents = mutateParents;
        }

        public int MaxGenerationSize { get; }
        public bool RandomizeMating { get; }
        public bool MutateParents { get; }
    }
}