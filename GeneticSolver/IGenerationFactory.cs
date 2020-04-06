using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenerationFactory<T>
    {
        T GetNewGenome();
        IEnumerable<T> CreateGeneration(int count);
        IEnumerable<T> CreateChildren(int count, T parentA, T parentB);
    }
}