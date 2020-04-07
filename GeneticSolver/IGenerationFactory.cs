using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenerationFactory<T>
    {
        T GetNewGenome();
    }
}