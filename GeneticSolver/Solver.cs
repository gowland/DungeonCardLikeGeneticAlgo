using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneticSolver.Genome;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public interface IRandom
    {
        double NextDouble();
        double NextDouble(double minX, double maxX);
    }

    public class UnWeightedRandom : IRandom
    {
        private readonly Random _rand = new Random();

        public double NextDouble()
        {
            return _rand.NextDouble();
        }

        public double NextDouble(double minX, double maxX)
        {
            return minX + NextDouble() * (maxX - minX);
        }
    }

    public class BellWeightedRandom : IRandom
    {
        private readonly Random _rand = new Random();
        private const double Mean = 0.5;
        private readonly Func<double, double, double> _genericBellFunc = (x, stdDev) => (1 / (stdDev * Math.Sqrt(2 * Math.PI))) * Math.Pow(Math.E, (x - Mean) * (x - Mean) / (2 * stdDev * stdDev));
        private readonly Func<double, double> _specificBellFunc;
        private readonly double _maxY;

        public BellWeightedRandom(double stdDev)
        {
            _specificBellFunc = x => _genericBellFunc(x, stdDev);
            _maxY = _specificBellFunc(Mean);
        }

        public double NextDouble()
        {
            while (true)
            {
                var x = _rand.NextDouble();
                var y = _rand.NextDouble() * _maxY;
                if (IsUnderCurve(x, y, _specificBellFunc))
    
                {
                    return x;
                }
            }
        }

        public double NextDouble(double minX, double maxX)
        {
            return minX + NextDouble() * (maxX - minX);
        }

        private bool IsUnderCurve(double x, double y, Func<double, double> func)
        {
            return y <= func(x);
        }
    }

    class ScoredGeneration<T, TScore>
        where TScore : IComparable<TScore>
    {
        public ScoredGeneration(IEnumerable<IGenomeInfo<T>> genomes, IGenomeEvaluator<T, TScore> evaluator)
        {
            IOrderedEnumerable<FitnessResult<T, TScore>> orderedFitnessResults = evaluator.GetFitnessResults(genomes);
            OrderedFitnessResults = orderedFitnessResults.ToArray();
        }

        public IEnumerable<IGenomeInfo<T>> OrderedGenomes => OrderedFitnessResults.Select(f => f.GenomeInfo);
        public IEnumerable<FitnessResult<T, TScore>> OrderedFitnessResults { get; }
    }

    public class Solver<T, TScore> 
        where T : ICloneable
        where TScore : IComparable<TScore>
    {
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeEvaluator<T, TScore> _evaluator;
        private readonly ISolverLogger<T, TScore> _logger;
        private readonly ISolverParameters _solverParameters;
        private readonly IEnumerable<IEarlyStoppingCondition<T, TScore>> _earlyStoppingConditions;
        private readonly IEnumerable<IGenomeReproductionStrategy<T>> _genomeReproductionStrategies;

        public Solver(IGenomeFactory<T> genomeFactory,
            IGenomeEvaluator<T, TScore> evaluator, 
            ISolverLogger<T, TScore> logger, ISolverParameters solverParameters, 
            IEnumerable<IEarlyStoppingCondition<T, TScore>> earlyStoppingConditions,
            IEnumerable<IGenomeReproductionStrategy<T>> genomeReproductionStrategies)
        {
            _genomeFactory = genomeFactory;
            _evaluator = evaluator;
            _logger = logger;
            _solverParameters = solverParameters;
            _earlyStoppingConditions = earlyStoppingConditions;
            _genomeReproductionStrategies = genomeReproductionStrategies;
        }

        public IGenerationResult<T, TScore> Evolve(int iterations, IEnumerable<T> originalGeneration = null)
        {
            GenerationResult<T, TScore> generationResult = null;

            var originalGenomes = originalGeneration ?? _genomeFactory.GetNewGenomes(_solverParameters.InitialGenerationSize);
            var scoredGeneration = new ScoredGeneration<T, TScore>(originalGenomes.Select(g => new GenomeInfo<T>(g, 0)), _evaluator);


            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                _logger.LogStartGeneration(generationNum);

                IEnumerable<IGenomeInfo<T>> elite = scoredGeneration.OrderedGenomes.Take(_solverParameters.MaxEliteSize);


                var num = generationNum;
                var children = _genomeReproductionStrategies
                    .SelectMany(reproductionStrategy =>
                        reproductionStrategy.ProduceOffspring(elite.Select(g => g.Genome)))
                    .Select(g => new GenomeInfo<T>(g, num));

                var nextGenerationGenomes = elite.Concat(children).ToArray();

                scoredGeneration = new ScoredGeneration<T, TScore>(nextGenerationGenomes, _evaluator);

                generationResult = new GenerationResult<T, TScore>(generationNum, scoredGeneration);

                _logger.LogGenerationInfo(generationResult);

                if (IsEarlyStopConditionHit(generationResult))
                {
                    return generationResult;
                }
            }

            return generationResult;
        }

        private bool IsEarlyStopConditionHit(GenerationResult<T, TScore> generationResult)
        {
            return _earlyStoppingConditions.Any(condition =>
                condition.Match(generationResult));
        }
    }
}