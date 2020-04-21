using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver.Interfaces
{
    public interface IBreadingStrategy
    {
        IEnumerable<Tuple<T, T>> GetPairs<T>(IOrderedEnumerable<T> genomes);
    }
}