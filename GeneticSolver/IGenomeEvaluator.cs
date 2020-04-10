using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public interface IGenomeEvaluator<T, TScore> where TScore : IComparable<TScore>
    {
        IOrderedEnumerable<FitnessResult<T, TScore>> GetFitnessResults(IEnumerable<IGenomeInfo<T>> genomes);
    }
}