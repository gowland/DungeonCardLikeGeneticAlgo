using System;
using GeneticSolver.Expressions;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.GenomeProperty
{
    public class ExpressionMutationVisitor<T> : IExpressionVisitor<T>
    {
        private readonly ExpressionGenerator<T> _expressionGenerator;
        private readonly IRandom _random;
        private readonly IValueSource<double> _mutationRate;
        private readonly IRandom _bellWeightedRandom;

        public ExpressionMutationVisitor(ExpressionGenerator<T> expressionGenerator, IRandom random, IValueSource<double> mutationRate, IRandom bellWeightedRandom)
        {
            _expressionGenerator = expressionGenerator;
            _random = random;
            _mutationRate = mutationRate;
            _bellWeightedRandom = bellWeightedRandom;
        }
        public void Visit(BoundValueExpression<T> expression)
        {
            // NOP
        }

        public void Visit(FuncExpression<T> expression)
        {
            if (_random.NextDouble() < _mutationRate.GetValue())
            {
                MutateFuncExpression(expression);
            }
        }

        public void Visit(ValueExpression<T> expression)
        {
            if (_random.NextDouble() < _mutationRate.GetValue())
            {
                MutateValueExpression(expression);
            }
        }

        private void MutateValueExpression(ValueExpression<T> expression)
        {
            expression.Value += _bellWeightedRandom.NextDouble(-5, 5);
        }

        private void MutateFuncExpression(FuncExpression<T> expression)
        {
            var rand = _random.NextDouble();
            if (rand < 0.33)
            {
                expression.Left = _expressionGenerator.GetRandomExpression();
            }
            else if (rand < 0.67)
            {
                expression.Right = _expressionGenerator.GetRandomExpression();
            }
            else
            {
                expression.Operation = _expressionGenerator.GetRandomOperation();
            }
        }
    }

    public class ExpressionGenomeProperty<T, TInput> : IGenomeProperty<T>
    {
        private readonly Func<T, IExpression<TInput>> _getterFunc;
        private readonly Action<T, IExpression<TInput>> _setterAction;
        private readonly ExpressionGenerator<TInput> _generator;
        private readonly IRandom _random = new UnWeightedRandom();
        private readonly ExpressionMutationVisitor<TInput> _mutationVisitor;

        public ExpressionGenomeProperty(Func<T, IExpression<TInput>> getterFunc, Action<T, IExpression<TInput>> setterAction, ExpressionGenerator<TInput> generator)
        {
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _generator = generator;
            _mutationVisitor = new ExpressionMutationVisitor<TInput>(generator, _random, new StaticValueSource<double>(0.3), new BellWeightedRandom(0.25));
        }
        public void Mutate(T genome)
        {
            var rand = _random.NextDouble();
            if (rand < 0.2)
            {
                _setterAction(genome, _generator.GetRandomExpression());
            }
            else if (rand < 0.6)
            {
                _setterAction(genome, GetWrappedExpression(_getterFunc(genome)));
            }
            else
            {
                _getterFunc(genome).Accept(_mutationVisitor);
            }
        }

        public void SetRandom(T genome)
        {
            _setterAction(genome, _generator.GetRandomExpression());
        }

        public void Merge(T parent1, T parent2, T child)
        {
            if (_random.NextDouble() < 0.5)
            {
                _setterAction(child, MergeExpressions(_getterFunc(parent1), _getterFunc(parent2)));
            }
            else
            {
                _setterAction(child, _random.NextDouble() < 0.5
                    ? _getterFunc(parent1)
                    : _getterFunc(parent2));
            }
        }

        private IExpression<TInput> GetWrappedExpression(IExpression<TInput> original)
        {
            return MergeExpressions(original, _generator.GetRandomExpression());
        }

        private IExpression<TInput> MergeExpressions(IExpression<TInput> original, IExpression<TInput> otherSide)
        {
            bool originalOnLeft = _random.NextDouble() > 0.5;
            return new FuncExpression<TInput>()
            {
                Left = originalOnLeft ? original : otherSide,
                Right = originalOnLeft ? otherSide : original,
                Operation = _generator.GetRandomOperation(),
            };
        }
    }
}