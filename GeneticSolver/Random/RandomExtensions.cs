using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Random
{
    public static class RandomExtensions
    {
        public static T SelectOption<T>(this IRandom random, IEnumerable<T> options)
        {
            return options.OrderBy(o => random.NextDouble()).FirstOrDefault();
        }

        public static T SelectOption<T>(this IRandom random, IEnumerable<Tuple<double,T>> options)
        {
            var rand = random.NextDouble();
            // var @join = string.Join(", ", options.Select(t => t.Item1).ToArray());
            // Console.WriteLine($"Random {rand} vs {@join}");
            return options.Where(t => rand <= t.Item1).OrderBy(t => t.Item1).Last().Item2;
        }
    }
}