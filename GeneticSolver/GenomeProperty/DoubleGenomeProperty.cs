using System;
using GeneticSolver.Expressions;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.GenomeProperty
{
    public class ExpressionGenomeProperty<T, TInput> : IGenomeProperty<T>
    {
        private readonly Func<T, IExpression<TInput>> _getterFunc;
        private readonly Action<T, IExpression<TInput>> _setterAction;
        private readonly ExpressionGenerator<TInput> _generator;
        private IRandom _random = new UnWeightedRandom();

        public ExpressionGenomeProperty(Func<T, IExpression<TInput>> getterFunc, Action<T, IExpression<TInput>> setterAction, ExpressionGenerator<TInput> generator)
        {
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _generator = generator;
        }
        public void Mutate(T genome)
        {
            // TODO: Mutate instead of generating from scratch
            _setterAction(genome, _generator.GetRandomExpression());
        }

        public void Mutate(T genome, IRandom random)
        {
            // TODO: Mutate instead of generating from scratch
            _setterAction(genome, _generator.GetRandomExpression());
        }

        public void SetRandom(T genome)
        {
            _setterAction(genome, _generator.GetRandomExpression());
        }

        public void Merge(T parent1, T parent2, T child)
        {
            _setterAction(child, _random.NextDouble() < 0.5
                ? _getterFunc(parent1)
                : _getterFunc(parent2));
        }
    }

    public class DoubleGenomeProperty<T> : IGenomeProperty<T>
    {
        private readonly IRandom _random;

        private readonly Func<T, double> _getterFunc;
        private readonly Action<T, double> _setterAction;
        private readonly double _min;
        private readonly double _max;
        private readonly double _minChange;
        private readonly double _maxChange;

        public DoubleGenomeProperty(Func<T, double> getterFunc, Action<T, double> setterAction, double min, double max, double minChange, double maxChange, IRandom random)
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
            _setterAction(genome, AlterNumber(_getterFunc(genome), _random));
        }

        public void Mutate(T genome, IRandom random)
        {
            _setterAction(genome, AlterNumber(_getterFunc(genome), random));
        }

        public void SetRandom(T genome)
        {
            _setterAction(genome, GetRandomDouble(_min, _max));
        }

        public void Merge(T parent1, T parent2, T child)
        {
            double parent1Percent = _random.NextDouble();
            double newValue = _getterFunc(parent1) * parent1Percent + _getterFunc(parent2) * (1- parent1Percent);
            _setterAction(child, newValue);
        }

        private double AlterNumber(double originalValue, IRandom random)
        {
            double newValue = originalValue + random.NextDouble(_minChange, _maxChange);
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