using System;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.GenomeProperty
{
    public class IntegerGenomeProperty<T> : IGenomeProperty<T>
    {
        private readonly System.Random _random;

        private readonly Func<T, int> _getterFunc;
        private readonly Action<T, int> _setterAction;
        private readonly int _min;
        private readonly int _max;
        private readonly int _minChange;
        private readonly int _maxChange;

        public IntegerGenomeProperty(Func<T, int> getterFunc, Action<T, int> setterAction, int min, int max, int minChange, int maxChange, System.Random random)
        {
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _min = min;
            _max = max;
            _minChange = minChange;
            _maxChange = maxChange;
            _random = random;
        }

        public void Mutate(T genome)
        {
            _setterAction(genome, AlterNumber(_getterFunc(genome)));
        }

        public void SetRandom(T genome)
        {
            _setterAction(genome, _random.Next(_min, _max + 1));
        }

        public void Merge(T parent1, T parent2, T child)
        {
            double parent1Percent = _random.NextDouble();
            var newValue = (int)(_getterFunc(parent1) * parent1Percent + _getterFunc(parent2) * (1- parent1Percent));
            _setterAction(child, newValue);
        }

        private int AlterNumber(int originalValue)
        {
            int newValue = originalValue + _random.Next(_minChange, _maxChange + 1);
            if (newValue < _min) newValue = _min;
            if (newValue > _max) newValue = _max;
            return newValue;
        }
    }
}