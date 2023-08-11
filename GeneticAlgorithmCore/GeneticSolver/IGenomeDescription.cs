using System.Collections.Generic;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public interface IGenomeDescription<in T>
    {
        IEnumerable<IGenomeProperty<T>> Properties { get; }
    }
}