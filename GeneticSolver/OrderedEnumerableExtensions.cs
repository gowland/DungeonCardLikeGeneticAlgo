using System;
using System.Linq;

namespace GeneticSolver
{
    public static class OrderedEnumerableExtensions
    {
        public static IOrderedEnumerable<T> Take<T>(this IOrderedEnumerable<T> items, int numToTake)
        {
            int count = 0;
            return Enumerable.Take(items, numToTake).OrderBy(_ => count++);
        }

        public static IOrderedEnumerable<U> Select<T, U>(this IOrderedEnumerable<T> items, Func<T, U> func)
        {
            int count = 0;
            return Enumerable.Select(items, func).OrderBy(_ => count++);
        }
    }
}