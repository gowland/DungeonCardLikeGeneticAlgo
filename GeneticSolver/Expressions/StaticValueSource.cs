namespace GeneticSolver.Expressions
{
    public class StaticValueSource<T> : IValueSource<T>
    {
        private readonly T _value;

        public StaticValueSource(T value)
        {
            _value = value;
        }
        public T GetValue()
        {
            return _value;
        }

        public static IValueSource<T> From(T value)
        {
            return new StaticValueSource<T>(value);
        }
    }
}