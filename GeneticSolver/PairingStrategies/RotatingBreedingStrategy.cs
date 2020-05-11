using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Interfaces;

namespace GeneticSolver.BreedingStrategies
{
    public class RotatingBreedingStrategy : IPairingStrategy
    {
        private readonly IPairingStrategy[] _strategies;
        private int _currentStrategy;

        public RotatingBreedingStrategy(IEnumerable<IPairingStrategy> strategies)
        {
            _strategies = strategies.ToArray();
            _currentStrategy = 0;
        }

        public IEnumerable<Tuple<T,T>> GetPairs<T>(IEnumerable<T> genomes)
        {
            var pairs = _strategies[_currentStrategy].GetPairs(genomes);
            _currentStrategy = (_currentStrategy + 1) % _strategies.Length;
            return pairs;
        }
    }
}