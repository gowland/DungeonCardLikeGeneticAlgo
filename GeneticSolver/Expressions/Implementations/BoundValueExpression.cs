using System;

namespace GeneticSolver.Expressions.Implementations
{
    public class BoundValueExpression<T> : IExpression<T>
    {
        private readonly Func<T, double> _valueSource;
        private readonly string _propertyName;

        public BoundValueExpression(Func<T, double> valueSource, string propertyName)
        {
            _valueSource = valueSource;
            _propertyName = propertyName;
        }
        public double Evaluate(T value)
        {
            return _valueSource(value);
        }

        public void Accept(IExpressionVisitor<T> visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{_propertyName}";
        }

        public object Clone()
        {
            return this;
        }
    }
}