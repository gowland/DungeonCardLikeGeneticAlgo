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
        private readonly IValueSource<double> _mutationProbability;
        private readonly IRandom _random;

        public GenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability, IRandom random)
            : this(genomeDescription, StaticValueSource<double>.From(mutationProbability), random)
        {
        }

        public GenomeMutator(IGenomeDescription<T> genomeDescription, IValueSource<double> mutationProbability, IRandom random)
        {
            _mutationProbability = mutationProbability;
            _genomeDescription = genomeDescription;
            _random = random;
        }

        public void Mutate(T genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() < _mutationProbability.GetValue()))
            {
                property.Mutate(genome);
            }
        }
    }

    public interface IValueSource<out T>
    {
        T GetValue();
    }

    public class DynamicValueSource<T> : IValueSource<T>
    {
        private readonly Func<T> _getterFunc;

        public DynamicValueSource(Func<T> getterFunc)
        {
            _getterFunc = getterFunc;
        }
        public T GetValue()
        {
            return _getterFunc();
        }
    }

    public class StaticValueSource<T> : IValueSource<T>
    {
        private readonly T _value;

        public StaticValueSource(T value)
        {
            _value = value;
        }
        public T GetValue()
        {
            return _value;
        }

        public static IValueSource<T> From(T value)
        {
            return new StaticValueSource<T>(value);
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