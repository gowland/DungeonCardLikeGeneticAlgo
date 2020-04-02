using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeRandomGenerator<T>
    {
        IEnumerable<IGenome<T>> CreateGeneration(int count);
        IEnumerable<IGenome<T>> CreateChildren(int count, IGenome<T> parentA, IGenome<T> parentB);

        IEnumerable<IGenome<T>> MutateGenomes(IEnumerable<IGenome<T>> genomes);
    }
}