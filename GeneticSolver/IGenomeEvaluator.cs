using System;
using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeEvaluator<T, TScore> where TScore : IComparable<TScore>
    {
        IEnumerable<FitnessResult<T, TScore>> GetFitnessResults(IEnumerable<IGenomeInfo<T>> genomes);
        IEnumerable<FitnessResult<T, TScore>> SortDescending(IEnumerable<FitnessResult<T, TScore>> genomes);
    }
}