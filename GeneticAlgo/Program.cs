using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using GeneticAlgo.Values;
using GeneticSolver;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.EarlyStoppingConditions;
using GeneticSolver.Genome;
using GeneticSolver.GenomeProperty;
using GeneticSolver.Interfaces;
using GeneticSolver.ReproductionStrategies;
using GeneticSolver.RequiredInterfaces;

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
            var genomeDescriptions = new CoefficientsGenomeDescriptions();
            var defaultGenomeFactory = new DefaultGenomeFactory<Coefficients>(genomeDescriptions);

            var solverParameters = new SolverParameters(
                10,
                20,
                0.3);

//            var tasks = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
//                tasks.Add(Task.Run(() => LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator)));
                LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator);
            }

//            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static void LaunchEvolutionRun(CoefficientsGenomeDescriptions genomeDescriptions,
            SolverParameters solverParameters, DefaultGenomeFactory<Coefficients> defaultGenomeFactory, CoefficientsGenomeEvaluator evaluator)
        {
            var mutator = new GenomeMutator<Coefficients>(genomeDescriptions, solverParameters.PropertyMutationProbability, new UnWeightedRandom());
            var logger = new CoefficientsSolverLogger();
            var solver = new Solver<Coefficients, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new IEarlyStoppingCondition<Coefficients, double>[]
                {
                    new FitnessThresholdReachedEarlyStopCondition<Coefficients, double>(fitness => fitness < 1e-6),
                    new ProgressStalledEarlyStoppingCondition<Coefficients, double>(100, 0.5, 0.8),
                    new FitnessNotImprovingEarlyStoppingCondition<Coefficients>(1e-6, 100),
                },
                new IGenomeReproductionStrategy<Coefficients>[]
                {
                    new SexualGenomeReproductionStrategy<Coefficients, double>(mutator, new HaremBreedingStrategy(),
                        defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
//                    new AsexualGenomeReproductionStrategy<Coefficients>(mutator), 
                });

            logger.Start();
            var best = solver.Evolve(1000);
            logger.LogGeneration(best);
            logger.End();
        }
    }

    public class Coefficients : ICloneable
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

        public object Clone()
        {
            return new Coefficients
            {
                FirstLevel = FirstLevel,
                SecondLevel = SecondLevel,
                ThirdLevel = ThirdLevel,
                FourthLevel = FourthLevel,
                FifthLevel = FifthLevel
            };
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
            return SortByDescendingFitness(genomes/*.AsParallel()*/.Select(genome => new FitnessResult<Coefficients, double>(genome, GetError(genome.Genome))));
        }

        public IOrderedEnumerable<Coefficients> GetFitnessResults(IEnumerable<Coefficients> genomes)
        {
            return genomes.OrderBy(GetError);
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
        private StreamWriter _logFile;
        private readonly Guid _loggerId = Guid.NewGuid();

        public void Start()
        {
            _logFile = new StreamWriter($"log_{_loggerId}.csv");
            _logFile.WriteLine("Run ID,Generation,Average Age,Top 10 Average Age,Best Age,Average Error,Top 10 Error, Best Error");
            Console.WriteLine($"{_loggerId} Start: {DateTime.Now:g}");
        }

        public void LogStartGeneration(int generationNumber)
        {
//            Console.WriteLine($"---------- {generationNumber} ---------- ");
        }

        public void LogGenerationInfo(IGenerationResult<Coefficients, double> generationResult)
        {
//            Console.WriteLine($" Average generation: {generationResult.AverageGenomeGeneration:0.00}");
//            Console.WriteLine($" Average fitness: {generationResult.OrderedGenomes.Average(r => r.Fitness):e2}");
            LogGenome(generationResult.FittestGenome);

            FitnessResult<Coefficients, double> topGenome = generationResult.FittestGenome;
            double averageAgeTop10Genomes = generationResult.OrderedGenomes.Take(10).Average(r => r.GenomeInfo.Generation);
            var averageScoreAllGenomes = generationResult.OrderedGenomes.Average(r => r.Fitness);
            var averageScoreTop10Genomes = generationResult.OrderedGenomes.Take(10).Average(r => r.Fitness);
            _logFile.WriteLine($"{_loggerId},{generationResult.GenerationNumber},{generationResult.AverageGenomeGeneration:0.00},{averageAgeTop10Genomes:0.00},{topGenome.GenomeInfo.Generation:0},{averageScoreAllGenomes:e2},{averageScoreTop10Genomes:e2},{topGenome.Fitness:e2}");
            _logFile.Flush();
        }

        private void LogGenome(FitnessResult<Coefficients, double> result)
        {
            var coefficients = result.GenomeInfo.Genome;
            Console.WriteLine($" {result.GenomeInfo.Generation}, {result.Fitness:e2} - ({coefficients.FifthLevel:0.##}) * x ^ 4 + ({coefficients.FourthLevel:0.##}) * x ^ 3 + ({coefficients.ThirdLevel:0.##}) * x ^ 2 + ({coefficients.SecondLevel:0.##}) * x + ({coefficients.FirstLevel:0.##})");
        }

        public void End()
        {
            Console.WriteLine($"{_loggerId} Fin: {DateTime.Now:g}");
            _logFile.Flush();
            _logFile.Close();
            _logFile = null;
        }

        public void LogGeneration(IGenerationResult<Coefficients, double> generationResult)
        {
            using (var generationFile = new StreamWriter($"generation_{generationResult.GenerationNumber}_{_loggerId}.csv"))
            {
                foreach (var fitnessResult in generationResult.OrderedGenomes)
                {
                    Coefficients genome = fitnessResult.GenomeInfo.Genome;
                    generationFile.WriteLine($"{fitnessResult.Fitness:e2},{fitnessResult.GenomeInfo.Generation},{genome.FifthLevel:0.0000},{genome.FourthLevel:0.0000},{genome.ThirdLevel:0.0000},{genome.SecondLevel:0.0000},{genome.FirstLevel:0.0000}");
                }
            }
        }
    }
}