using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Expressions
{
    public class ExpressionGenerator
    {
        private readonly IRandom _random;
        private readonly IEnumerable<IExpression> _boundValueExpressions;
        private readonly IEnumerable<Operation> _operations;

        public ExpressionGenerator(IRandom random, IEnumerable<IExpression> boundValueExpressions, IEnumerable<Operation> operations)
        {
            _random = random;
            _boundValueExpressions = boundValueExpressions;
            _operations = operations;
        }
        public IExpression GetRandomExpression()
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

        public IExpression GetFunctionExpression()
        {
            return new FuncExpression()
            {
                Left = GetRandomExpression(),
                Right = GetRandomExpression(),
                Operation = _operations.OrderBy(f => _random.NextDouble()).First(),
            };
        }

        public IExpression GetRandomValueExpression()
        {
            return _random.NextDouble() < 0.5
                ? GetBoundValueExpression()
                : GetMutableValueExpression();
        }

        public IExpression GetBoundValueExpression()
        {
            return _boundValueExpressions.OrderBy(e => _random.NextDouble()).First();
        }

        public IExpression GetMutableValueExpression()
        {
            return new ValueExpression(_random.NextDouble(-20,20));
        }
    }

    public interface IExpression : ICloneable
    {
        double Evaluate();
        void Accept(IExpressionVisitor visitor);
    }

    public interface IExpressionVisitor
    {}

    public class ValueExpression : IExpression
    {
        private double _value;

        public ValueExpression(double value)
        {
            _value = value;
        }
        public double Evaluate()
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
            return new ValueExpression(_value);
        }
    }

    public class BoundValueExpression : IExpression
    {
        private readonly IValueSource<double> _valueSource;

        public BoundValueExpression(IValueSource<double> valueSource)
        {
            _valueSource = valueSource;
        }
        public double Evaluate()
        {
            return _valueSource.GetValue();
        }

        public void Accept(IExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{_valueSource.GetValue():00.00000}";
        }

        public object Clone()
        {
            return new BoundValueExpression(_valueSource);
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

    public class FuncExpression : IExpression
    {
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }
        public Operation Operation { get; set; }

        public double Evaluate()
        {
            return Operation.Function(Left.Evaluate(), Right.Evaluate());
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
            return new FuncExpression()
            {
                Left = (IExpression)Left.Clone(),
                Right = (IExpression)Right.Clone(),
                Operation = Operation,
            };
        }
    }
}