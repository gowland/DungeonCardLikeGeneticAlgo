using System;
using System.Collections.Generic;

namespace GeneticSolver.Interfaces
{
    public interface IPairingStrategy
    {
        IEnumerable<Tuple<T, T>> GetPairs<T>(IEnumerable<T> genomes); // TODO: Many of these strategies depend on the results being ordered
    }
}