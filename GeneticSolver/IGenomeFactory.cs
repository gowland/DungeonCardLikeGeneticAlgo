using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeFactory<T>
    {
        T GetNewGenome();
    }
}