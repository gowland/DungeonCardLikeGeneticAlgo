using System;
using System.Collections.Generic;

namespace GeneticSolver.Interfaces
{
    public interface IPairingStrategy
    {
        IEnumerable<Tuple<T, T>> GetPairs<T>(IEnumerable<T> genomes);
    }
}