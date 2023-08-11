using System;
using System.Collections.Generic;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IPairingStrategy<T>
    {
        IEnumerable<Tuple<T, T>> GetPairs(IEnumerable<T> items);
    }
}