using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class GenomeMutator<T> : IMutator<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly double _mutationProbability;
        private readonly IRandom _random;

        public GenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability, IRandom random)
        {
            _genomeDescription = genomeDescription;
            _mutationProbability = mutationProbability;
            _random = random;
        }

        public void Mutate(T genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() < _mutationProbability))
            {
                property.Mutate(genome);
            }
        }
    }

    public class BellWeightedGenomeMutator<T> : IMutator<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly double _mutationProbability;
        private static readonly double[] _stdDeviationsCycle = {10, 1, 0.1, 0.01, 0.001, 0.0001};
        private Queue<double> currentQueue = new Queue<double>(_stdDeviationsCycle);
        private Queue<double> usedValueQueue = new Queue<double>();
        private IRandom _random;

        public BellWeightedGenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability)
        {
            _genomeDescription = genomeDescription;
            _mutationProbability = mutationProbability;
            _random = new BellWeightedRandom(1);
        }

        public void CycleStdDev()
        {
            if (currentQueue.Count <= 0)
            {
                var tmp = currentQueue;
                currentQueue = usedValueQueue;
                usedValueQueue = tmp;
            }

            double currentStdDev = currentQueue.Dequeue();
            usedValueQueue.Enqueue(currentStdDev);
            _random = new BellWeightedRandom(currentStdDev);
        }

        public void Mutate(T genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() < _mutationProbability))
            {
                property.Mutate(genome);
            }
        }
    }
}