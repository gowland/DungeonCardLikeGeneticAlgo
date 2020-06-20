using System;
using System.Linq;
using GeneticSolver.GenomeProperty;

namespace GeneticSolver.Expressions
{
    public interface IExpression<T> : ICloneable
    {
        double Evaluate(T input);
        void Accept(IExpressionVisitor<T> visitor);
    }
}