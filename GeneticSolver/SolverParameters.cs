using System.Collections.Generic;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxGenerationSize, bool mutateParents, double propertyMutationProbability, IBreadingStrategy breadingStrategy)
        {
            MaxGenerationSize = maxGenerationSize;
            MutateParents = mutateParents;
            PropertyMutationProbability = propertyMutationProbability;
            BreadingStrategy = breadingStrategy;
        }

        public int MaxGenerationSize { get; }
        public bool MutateParents { get; }
        public double PropertyMutationProbability { get; }
        public IBreadingStrategy BreadingStrategy { get; }
    }
}