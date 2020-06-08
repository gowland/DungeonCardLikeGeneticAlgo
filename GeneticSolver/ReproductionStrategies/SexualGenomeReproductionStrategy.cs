using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Genome;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.ReproductionStrategies
{
    public class SexualGenomeReproductionStrategy<T, TScore> : IGenomeReproductionStrategy<T>
        where T : class, ICloneable
        where TScore : IComparable<TScore>
    {
        private readonly IMutator<T> _mutator;
        private readonly IPairingStrategy _pairingStrategy;
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly IGenomeEvaluator<T, TScore> _genomeEvaluator;
        private readonly int _childrenToCreate;
        private readonly int _childrenToKeepPerPair;

        public SexualGenomeReproductionStrategy(
            IMutator<T> mutator,
            IPairingStrategy pairingStrategy,
            IGenomeFactory<T> genomeFactory,
            IGenomeDescription<T> genomeDescription,
            IGenomeEvaluator<T, TScore> genomeEvaluator,
            int childrenToCreate,
            int childrenToKeepPerPair)
        {
            _mutator = mutator;
            _pairingStrategy = pairingStrategy;
            _genomeFactory = genomeFactory;
            _genomeDescription = genomeDescription;
            _genomeEvaluator = genomeEvaluator;
            _childrenToCreate = childrenToCreate;
            _childrenToKeepPerPair = childrenToKeepPerPair;
        }

        public IEnumerable<T> ProduceOffspring(IEnumerable<T> parents)
        {
            var nextGen = _pairingStrategy.GetPairs(parents)
//                .AsParallel()
                .Select(pair => CreateChildren(pair.Item1, pair.Item2))
                .SelectMany(TakeFittest)
                .ToList();

            return nextGen;
        }

        private IEnumerable<T> CreateChildren(T parentA, T parentB)
        {
            for (int i = 0; i < _childrenToCreate; i++)
            {
                var child = _genomeFactory.GetNewGenome();

                foreach (var property in _genomeDescription.Properties)
                {
                    property.Merge(parentA, parentB, child);
                }

                _mutator.Mutate(child);

                yield return child;
            }
        }

        private IEnumerable<T> TakeFittest(IEnumerable<T> genomes)
        {
            return _genomeEvaluator
                .GetFitnessResults(genomes)
                .Take(_childrenToKeepPerPair);
        }
    }
}