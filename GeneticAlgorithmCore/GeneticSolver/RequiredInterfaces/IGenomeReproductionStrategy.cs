using System.Collections.Generic;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IGenomeReproductionStrategy<T>
    {
        IEnumerable<T> ProduceOffspring(IEnumerable<T> parents);
    }
}