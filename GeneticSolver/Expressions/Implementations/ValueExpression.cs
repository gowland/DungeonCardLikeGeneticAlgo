namespace GeneticSolver.Expressions.Implementations
{
    public class ValueExpression<T> : IExpression<T>
    {
        public double Value { get; set; }

        public ValueExpression(double value)
        {
            Value = value;
        }
        public double Evaluate(T value)
        {
            return Value;
        }

        public void Accept(IExpressionVisitor<T> visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{Value:00.00000}";
        }

        public object Clone()
        {
            return new ValueExpression<T>(Value);
        }
    }
}