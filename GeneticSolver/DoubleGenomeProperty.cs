using System;

namespace GeneticSolver
{
    public class DoubleGenomeProperty<T> : IGenomeProperty<T>
    {
        private readonly Random _random;

        private readonly Func<T, double> _getterFunc;
        private readonly Action<T, double> _setterAction;
        private readonly double _min;
        private readonly double _max;
        private readonly int _minChange;
        private readonly int _maxChange;

        public DoubleGenomeProperty(Func<T, double> getterFunc, Action<T, double> setterAction, double min, double max, int minChange, int maxChange, Random random)
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
            _setterAction(genome, GetRandomDouble(_min, _max));
        }

        public void Merge(T parent1, T parent2, T child)
        {
            double parent1Percent = _random.NextDouble();
            var newValue = (int)(_getterFunc(parent1) * parent1Percent + _getterFunc(parent2) * (1- parent1Percent));
            _setterAction(child, newValue);
        }

        private double AlterNumber(double originalValue)
        {
            double newValue = originalValue + GetRandomDouble(_minChange, _maxChange);
            if (newValue < _min) newValue = _min;
            if (newValue > _max) newValue = _max;
            return newValue;
        }

        private double GetRandomDouble(double min, double max)
        {
            double range = Math.Abs(max - min);
            return _random.NextDouble() * range + min;
        }
    }
}