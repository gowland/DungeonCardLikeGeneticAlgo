using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeFactory<out T>
    {
        T GetNewGenome();
        IEnumerable<T> GetNewGenomes(int count);
    }
}