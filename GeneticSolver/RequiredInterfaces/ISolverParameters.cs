using System.Collections.Generic;

namespace GeneticSolver.RequiredInterfaces
{
    public interface ISolverParameters
    {
        int MaxGenerationSize { get; }
        bool RandomizeMating { get; }
        bool MutateParents { get; }
        double PropertyMutationProbability { get; }
    }
}