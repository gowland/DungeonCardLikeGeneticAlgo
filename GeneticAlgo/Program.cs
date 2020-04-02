using System;
using System.Linq;
using GeneticAlgo.Values;
using GeneticSolver;

namespace GeneticAlgo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var evolver = new Solver<Values.Values>();
            ConsoleKeyInfo key = new ConsoleKeyInfo(' ', ConsoleKey.A, false, false, false);
            while (key.Key != ConsoleKey.X)
            {
                var best = evolver.Evolve(
                    new ValuesGenomeRandomGenerator(), new ValuesGenomeEvaluator(), 100, 1000);

                Console.WriteLine($"Best = {best.Value.Sum}");
                key = Console.ReadKey();
            }
        }
    }
}