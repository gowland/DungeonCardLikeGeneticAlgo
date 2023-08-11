using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IGenomeEvaluator<T, TScore> where TScore : IComparable<TScore>
    {
        IOrderedEnumerable<FitnessResult<T, TScore>> GetFitnessResults(IEnumerable<IGenomeInfo<T>> genomes);
        IOrderedEnumerable<T> GetFitnessResults(IEnumerable<T> genomes);
    }
}