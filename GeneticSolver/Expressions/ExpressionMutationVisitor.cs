using GeneticSolver.Expressions.Implementations;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Expressions
{
    public class ExpressionMutationVisitor<T> : IExpressionVisitor<T>
    {
        private readonly ExpressionGenerator<T> _expressionGenerator;
        private readonly IRandom _random;
        private readonly IValueSource<double> _mutationRate;
        private readonly IRandom _bellWeightedRandom;

        public ExpressionMutationVisitor(ExpressionGenerator<T> expressionGenerator, IRandom random,
            double mutationRate, IRandom bellWeightedRandom)
            : this(expressionGenerator, random, new StaticValueSource<double>(mutationRate), bellWeightedRandom)
        {
        }

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
}