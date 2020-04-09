using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public interface IGenomeEvaluator<T, TScore> where TScore : IComparable<TScore>
    {
        IEnumerable<FitnessResult<T, TScore>> GetFitnessResults(IEnumerable<IGenomeInfo<T>> genomes);
        IOrderedEnumerable<FitnessResult<T, TScore>> SortByDescendingFitness(IEnumerable<FitnessResult<T, TScore>> genomes);
    }
}