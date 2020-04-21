using System.Collections.Generic;
using GeneticSolver.Interfaces;

namespace GeneticSolver.RequiredInterfaces
{
    public interface ISolverParameters
    {
        int MaxGenerationSize { get; }
        bool MutateParents { get; }
        double PropertyMutationProbability { get; }
        IBreadingStrategy BreadingStrategy { get; }
    }
}