using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeDescription<in T>
    {
        IEnumerable<IGenomeProperty<T>> Properties { get; }
    }
}