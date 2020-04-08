namespace GeneticSolver
{
    public interface ISolverParameters
    {
        int MaxGenerationSize { get; }
        bool RandomizeMating { get; }
        bool MutateParents { get; }
    }
}