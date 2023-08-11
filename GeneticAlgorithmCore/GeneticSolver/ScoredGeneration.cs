using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class ScoredGeneration<T, TScore>
        where TScore : IComparable<TScore>
    {
        public ScoredGeneration(IEnumerable<IGenomeInfo<T>> genomes, IGenomeEvaluator<T, TScore> evaluator)
        {
            IOrderedEnumerable<FitnessResult<T, TScore>> orderedFitnessResults = evaluator.GetFitnessResults(genomes);
            OrderedFitnessResults = orderedFitnessResults.ToArray();
        }

        public IEnumerable<IGenomeInfo<T>> OrderedGenomes => OrderedFitnessResults.Select(f => f.GenomeInfo);
        public IEnumerable<FitnessResult<T, TScore>> OrderedFitnessResults { get; }
    }
}