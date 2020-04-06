using System;
using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenomeDescription<in T>
    {
        IEnumerable<IGenomeProperty<T>> Properties { get; }
    }

    public interface IGenomeProperty<in T>
    {
        void Mutate(T value);
        void SetRandom(T value);
    }

    public class IntegerGenomeProperty<T> : IGenomeProperty<T>
    {
        private readonly Random _random;

        private readonly Func<T, int> _getterFunc;
        private readonly Action<T, int> _setterAction;
        private readonly int _min;
        private readonly int _max;
        private readonly int _minChange;
        private readonly int _maxChange;

        public IntegerGenomeProperty(Func<T, int> getterFunc, Action<T, int> setterAction, int min, int max, int minChange, int maxChange, Random random)
        {
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _min = min;
            _max = max;
            _minChange = minChange;
            _maxChange = maxChange;
            _random = random;
        }

        public void Mutate(T value)
        {
            _setterAction(value, AlterNumber(_getterFunc(value)));
        }

        public void SetRandom(T value)
        {
            _setterAction(value, _random.Next(_min, _max + 1));
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