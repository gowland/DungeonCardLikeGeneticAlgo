using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenerationFactory<T>
    {
        IEnumerable<T> CreateGeneration(int count);
        IEnumerable<T> CreateChildren(int count, T parentA, T parentB);
        IEnumerable<T> MutateGenomes(IEnumerable<T> genomes);
    }
}