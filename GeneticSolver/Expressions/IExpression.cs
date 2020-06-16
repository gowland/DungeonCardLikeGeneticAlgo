using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Expressions
{
    public class ExpressionGenerator<T>
    {
        private readonly IRandom _random;
        private readonly IEnumerable<IExpression<T>> _boundValueExpressions;
        private readonly IEnumerable<Operation> _operations;

        public ExpressionGenerator(IRandom random, IEnumerable<IExpression<T>> boundValueExpressions, IEnumerable<Operation> operations)
        {
            _random = random;
            _boundValueExpressions = boundValueExpressions;
            _operations = operations;
        }
        public IExpression<T> GetRandomExpression()
        {
            var randVal = _random.NextDouble();

            if (randVal < 0.33)
            {
                return GetFunctionExpression();
            }
            else if (randVal < 0.66)
            {
                return GetBoundValueExpression();
            }
            else
            {
                return GetMutableValueExpression();
            }
        }

        public IExpression<T> GetFunctionExpression()
        {
            return new FuncExpression<T>()
            {
                Left = GetRandomExpression(),
                Right = GetRandomExpression(),
                Operation = _operations.OrderBy(f => _random.NextDouble()).First(),
            };
        }

        public IExpression<T> GetRandomValueExpression()
        {
            return _random.NextDouble() < 0.5
                ? GetBoundValueExpression()
                : GetMutableValueExpression();
        }

        public IExpression<T> GetBoundValueExpression()
        {
            return _boundValueExpressions.OrderBy(e => _random.NextDouble()).First();
        }

        public IExpression<T> GetMutableValueExpression()
        {
            return new ValueExpression<T>(_random.NextDouble(-20,20));
        }
    }

    public interface IExpression<T> : ICloneable
    {
        double Evaluate(T input);
        void Accept(IExpressionVisitor visitor);
    }

    public interface IExpressionVisitor
    {}

    public class ValueExpression<T> : IExpression<T>
    {
        private double _value;

        public ValueExpression(double value)
        {
            _value = value;
        }
        public double Evaluate(T value)
        {
            return _value;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{_value:00.00000}";
        }

        public object Clone()
        {
            return new ValueExpression<T>(_value);
        }
    }

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

        public void Accept(IExpressionVisitor visitor)
        {
            throw new NotImplementedException();
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

    public class Operation
    {
        private readonly string _symbol;

        public Operation(Func<double, double, double> func, string symbol)
        {
            Function = func;
            _symbol = symbol;
        }

        public Func<double, double, double> Function { get; }

        public override string ToString()
        {
            return _symbol;
        }
    }

    public class FuncExpression<T> : IExpression<T>
    {
        public IExpression<T> Left { get; set; }
        public IExpression<T> Right { get; set; }
        public Operation Operation { get; set; }

        public double Evaluate(T value)
        {
            return Operation.Function(Left.Evaluate(value), Right.Evaluate(value));
        }

        public void Accept(IExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"({Left}) {Operation} ({Right})";
        }

        public object Clone()
        {
            return new FuncExpression<T>()
            {
                Left = (IExpression<T>)Left.Clone(),
                Right = (IExpression<T>)Right.Clone(),
                Operation = Operation,
            };
        }
    }
}