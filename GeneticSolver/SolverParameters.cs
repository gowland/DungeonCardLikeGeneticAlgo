using System.Collections.Generic;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxEliteSize, int initialGenerationSize, bool mutateParents,
            double propertyMutationProbability, IPairingStrategy pairingStrategy)
        {
            MaxEliteSize = maxEliteSize;
            MutateParents = mutateParents;
            PropertyMutationProbability = propertyMutationProbability;
            PairingStrategy = pairingStrategy;
            InitialGenerationSize = initialGenerationSize;
        }

        public int MaxEliteSize { get; }
        public int InitialGenerationSize { get; }
        public bool MutateParents { get; }
        public double PropertyMutationProbability { get; }
        public IPairingStrategy PairingStrategy { get; }
    }
}