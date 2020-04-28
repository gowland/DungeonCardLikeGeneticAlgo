namespace GeneticSolver.RequiredInterfaces
{
    public interface ISolverParameters
    {
        int MaxEliteSize { get; }
        int InitialGenerationSize { get; }
        double PropertyMutationProbability { get; }
    }
}