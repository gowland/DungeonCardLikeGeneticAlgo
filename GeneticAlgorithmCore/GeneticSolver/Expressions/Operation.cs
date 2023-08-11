using System;

namespace GeneticSolver.Expressions
{
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
}