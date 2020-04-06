using System;
using System.Collections.Generic;

namespace GeneticSolver
{
    public interface IGenome<out T> : IGenome
    {
        T Value { get; }
    }

    public interface IGenome
    {
        IEnumerable<IGenomeProperty> Properties { get; }
    }

    public interface IGenomeProperty
    {
        void Mutate();
        void SetRandom();
    }

    public class IntegerGenomeProperty : IGenomeProperty
    {
        private readonly Random _random = new Random();

        private readonly Func<int> _getterFunc;
        private readonly Action<int> _setterAction;
        private readonly int _min;
        private readonly int _max;
        private readonly int _minChange;
        private readonly int _maxChange;

        public IntegerGenomeProperty(Func<int> getterFunc, Action<int> setterAction, int min, int max, int minChange, int maxChange)
        {
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _min = min;
            _max = max;
            _minChange = minChange;
            _maxChange = maxChange;
        }

        public void Mutate()
        {
            _setterAction(AlterNumber(_getterFunc()));
        }

        public void SetRandom()
        {
            _setterAction(_random.Next(_min, _max + 1));
        }

        private int AlterNumber(int originalValue)
        {
            int change = _random.Next(_minChange, _maxChange + 1);
            int newValue = originalValue + change;
            if (newValue < _min) newValue = _min;
            if (newValue > _max) newValue = _max;
            return newValue;
        }
    }
}