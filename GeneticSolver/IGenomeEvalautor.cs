using System;
using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeEvalautor<T, TScore> where TScore : IComparable<TScore>
    {
        IEnumerable<FitnessResult<T, TScore>> GetFitnessResults(IEnumerable<IGenomeInfo<T>> genomes);
    }
}