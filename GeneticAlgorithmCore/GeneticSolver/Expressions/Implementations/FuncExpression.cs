namespace GeneticSolver.Expressions.Implementations
{
    public class FuncExpression<T> : IExpression<T>
    {
        public IExpression<T> Left { get; set; }
        public IExpression<T> Right { get; set; }
        public Operation Operation { get; set; }

        public double Evaluate(T value)
        {
            return Operation.Function(Left.Evaluate(value), Right.Evaluate(value));
        }

        public void Accept(IExpressionVisitor<T> visitor)
        {
            visitor.Visit(this);
            Left.Accept(visitor);
            Right.Accept(visitor);
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