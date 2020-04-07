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
            var genomeDescription = new ValuesGenomeDescription();
            var solver = new Solver<Values.Values>(new ValuesGenerationFactory(), new ValuesGenomeEvaluator(), genomeDescription);
            ConsoleKeyInfo key = new ConsoleKeyInfo(' ', ConsoleKey.A, false, false, false);
            while (key.Key != ConsoleKey.X && key.Key != ConsoleKey.Q && key.Key != ConsoleKey.Escape)
            {
                var best = solver.Solve(100, 1000);

                Console.WriteLine($"Best = {best.Sum}");
                key = Console.ReadKey();
            }
        }
    }
}