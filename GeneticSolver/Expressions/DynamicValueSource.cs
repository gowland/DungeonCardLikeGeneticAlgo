using System;

namespace GeneticSolver.Expressions
{
    public class DynamicValueSource<T> : IValueSource<T>
    {
        private readonly Func<T> _getterFunc;

        public DynamicValueSource(Func<T> getterFunc)
        {
            _getterFunc = getterFunc;
        }
        public T GetValue()
        {
            return _getterFunc();
        }
    }
}