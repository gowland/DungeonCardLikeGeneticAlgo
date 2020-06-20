using System;
using GeneticSolver.Expressions;
using GeneticSolver.Expressions.Implementations;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.GenomeProperty
{
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
            _mutationVisitor = new ExpressionMutationVisitor<TInput>(generator, _random, 0.3, new BellWeightedRandom(0.25));
        }
        public void Mutate(T genome)
        {
            _random.SelectOption(new[]
            {
                new Tuple<double,Action>(0.2, () => _setterAction(genome, _generator.GetRandomExpression())),
                new Tuple<double,Action>(0.6, () => _setterAction(genome, GetWrappedExpression(_getterFunc(genome)))),
                new Tuple<double,Action>(1.0, () => _getterFunc(genome).Accept(_mutationVisitor)),
            }).Invoke();
        }

        public void SetRandom(T genome)
        {
            _setterAction(genome, _generator.GetRandomExpression());
        }

        public void Merge(T parent1, T parent2, T child)
        {
            _random.SelectOption(new Action[]
            {
                () => _setterAction(child, MergeExpressions(
                    (IExpression<TInput>)_getterFunc(parent1).Clone(),
                    (IExpression<TInput>)_getterFunc(parent2).Clone())),
                () => _setterAction(child, _random.NextBool()
                    ? (IExpression<TInput>)_getterFunc(parent1).Clone()
                    : (IExpression<TInput>)_getterFunc(parent2).Clone()),
            }).Invoke();
        }

        private IExpression<TInput> GetWrappedExpression(IExpression<TInput> original)
        {
            return MergeExpressions(original, _generator.GetRandomExpression());
        }

        private IExpression<TInput> MergeExpressions(IExpression<TInput> a, IExpression<TInput> b)
        {
            bool aOnLeft = _random.NextBool();
            return new FuncExpression<TInput>()
            {
                Left = aOnLeft ? a : b,
                Right = aOnLeft ? b : a,
                Operation = _generator.GetRandomOperation(),
            };
        }
    }
}