using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticAlgo.Values;
using GeneticSolver;

namespace GeneticAlgo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
//            var solver = new Solver<Values.MyThing, int>(new DefaultGenomeFactory<MyThing>(), new MyThingGenomeEvaluator(), new MyThingGenomeDescription(), new MyThingSolverLogger(), new SolverParameters(1000));

            var coefficientsToMatch = new Coefficients
            {
                FifthLevel = 0,
                FourthLevel = 3.9,
                ThirdLevel = -34.5,
                SecondLevel = -9.8,
                FirstLevel = 11.9
            };
            var pointsToMatch = Enumerable.Range(-1000,1000).Select(x => new Point(x, coefficientsToMatch.Calc(x)));
            var evaluator = new CoefficientsGenomeEvaluator(pointsToMatch);
            var solverParameters = new SolverParameters(5000, false, false, 0.3);
            var logger = new CoefficientsSolverLogger();
            var solver = new Solver<Coefficients, double>(new DefaultGenomeFactory<Coefficients>(), evaluator, new CoefficientsGenomeDescriptions(), logger, solverParameters);

            ConsoleKeyInfo key = new ConsoleKeyInfo(' ', ConsoleKey.A, false, false, false);
            while (key.Key != ConsoleKey.X && key.Key != ConsoleKey.Q && key.Key != ConsoleKey.Escape)
            {
                logger.Start(solverParameters);
                var best = solver.Evolve(1000);
                logger.End();

                key = Console.ReadKey();
            }
        }

    }


    public class Coefficients
    {
        public double FifthLevel { get; set; }
        public double FourthLevel { get; set; }
        public double ThirdLevel { get; set; }
        public double SecondLevel { get; set; }
        public double FirstLevel { get; set; }

        public double Calc(double x)
        {
            return FifthLevel * Math.Pow(x, 4)
                   + FourthLevel * Math.Pow(x, 3)
                   + ThirdLevel * Math.Pow(x, 2)
                   + SecondLevel * Math.Pow(x, 1)
                   + FirstLevel;
        }
    }

    public class CoefficientsGenomeDescriptions : IGenomeDescription<Coefficients>
    {
        private readonly Random _random = new Random();
        private double _minChange = -5;
        private double _maxChange = 5;

        public IEnumerable<IGenomeProperty<Coefficients>> Properties => new[]
        {
            new DoubleGenomeProperty<Coefficients>(g => g.FifthLevel, (g, val) => g.FifthLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.FourthLevel, (g, val) => g.FourthLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.ThirdLevel, (g, val) => g.ThirdLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.SecondLevel, (g, val) => g.SecondLevel = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<Coefficients>(g => g.FirstLevel, (g, val) => g.FirstLevel = val, -100, 100, _minChange, _maxChange, _random),
        };
    }

    public class Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public class CoefficientsGenomeEvaluator : IGenomeEvaluator<Coefficients, double>
    {
        private readonly ICollection<Point> _pointsToMatch;

        public CoefficientsGenomeEvaluator(IEnumerable<Point> pointsToMatch)
        {
            _pointsToMatch = pointsToMatch.ToArray();
        }

        public IOrderedEnumerable<FitnessResult<Coefficients, double>> GetFitnessResults(IEnumerable<IGenomeInfo<Coefficients>> genomes)
        {
            return SortByDescendingFitness(genomes.Select(genome => new FitnessResult<Coefficients, double>(genome, GetError(genome.Genome))));
        }

        private IOrderedEnumerable<FitnessResult<Coefficients, double>> SortByDescendingFitness(IEnumerable<FitnessResult<Coefficients, double>> genomes)
        {
            return genomes.OrderBy(g => g.Fitness);
        }

        private double GetError(Coefficients coefficients)
        {
            return _pointsToMatch
                .Select(point => (point.Y - coefficients.Calc(point.X))/point.Y)
                .Average(error => error * error);
//                return _pointsToMatch
//                    .Average(point => Math.Abs(point.Y - coefficients.Calc(point.X))/point.Y);
        }
    }

    public class CoefficientsSolverLogger : ISolverLogger<Coefficients, double>
    {
        private StreamWriter logFile;

        public void Start(ISolverParameters parameters)
        {
            var filename = $"log_{DateTime.Now:s}";
            logFile = new StreamWriter("log.csv");
            logFile.WriteLine($"Generation Size,{parameters.MaxGenerationSize}");
            logFile.WriteLine($"Mutation Probability,{parameters.PropertyMutationProbability:0.00}");
            logFile.WriteLine($"Mutate Parents,{(parameters.MutateParents ? "YES" : "NO")}");
            logFile.WriteLine($"Random Mating,{(parameters.RandomizeMating ? "YES" : "NO")}");
            logFile.WriteLine();
            logFile.WriteLine("Generation,Average Age,Top 10 Average Age,Best Age,Average Error,Top 10 Error, Best Error");
        }

        public void LogStartGeneration(int generationNumber)
        {
            Console.WriteLine($"---------- {generationNumber} ---------- ");
        }

        public void LogGenerationInfo(int generationNumber, IOrderedEnumerable<FitnessResult<Coefficients, double>> results)
        {
            var resultsArr = results.ToArray();

            double averageAgeAllGenomes = resultsArr.Average(r => r.GenomeInfo.Generation);
            double averageAgeTop10Genomes = resultsArr.Take(10).Average(r => r.GenomeInfo.Generation);

            var averageScoreAllGenomes = resultsArr.Average(r => r.Fitness);
            var averageScoreTop10Genomes = resultsArr.Take(10).Average(r => r.Fitness);

            FitnessResult<Coefficients, double> topGenome = resultsArr.First();

            Console.WriteLine($" Average age: {averageAgeAllGenomes:0.00}");
            Console.WriteLine($" Average score: {averageScoreAllGenomes:e2}");
            LogGenome(topGenome);

            logFile.WriteLine($"{generationNumber},{averageAgeAllGenomes:0.00},{averageAgeTop10Genomes:0.00},{topGenome.GenomeInfo.Generation:0},{averageScoreAllGenomes:e2},{averageScoreTop10Genomes:e2},{topGenome.Fitness:e2}");
        }

        private void LogGenome(FitnessResult<Coefficients, double> result)
        {
            var coefficients = result.GenomeInfo.Genome;
            Console.WriteLine($" {result.GenomeInfo.Generation}, {result.Fitness:e2} - ({coefficients.FifthLevel:0.##}) * x ^ 4 + ({coefficients.FourthLevel:0.##}) * x ^ 3 + ({coefficients.ThirdLevel:0.##}) * x ^ 2 + ({coefficients.SecondLevel:0.##}) * x + ({coefficients.FirstLevel:0.##})");
        }

        public void End()
        {
            logFile.Flush();
            logFile = null;
        }
    }
}