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
        private readonly Random _random = new Random();

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

        public GenerationResult<T, TScore> Evolve(int iterations, IEnumerable<T> originalGeneration = null)
        {
            GenerationResult<T, TScore> generationResult = null;

            var generation = _evaluator.GetFitnessResults(
             originalGeneration?.Select(g => new GenomeInfo<T>(g, 0))
                ?? CreateGeneration(_solverParameters.InitialGenerationSize));

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                _logger.LogStartGeneration(generationNum);

                IOrderedEnumerable<IGenomeInfo<T>> elite = SelectFittest(generation, _solverParameters.MaxEliteSize);

                var num = generationNum;
                var children = _genomeReproductionStrategies
                    .SelectMany(reproductionStrategy =>
                        reproductionStrategy.ProduceOffspring(elite.Select(g => g.Genome)))
                    .Select(g => new GenomeInfo<T>(g, num));

                var nextGenerationGenomes = elite.Concat(children).ToArray();

                generation = _evaluator.GetFitnessResults(nextGenerationGenomes);

                generationResult = new GenerationResult<T, TScore>(generationNum, generation);

                _logger.LogGenerationInfo(generationResult);

                if (_earlyStoppingConditions.Any(condition =>
                    condition.Match(generationResult)))
                {
                    return generationResult;
                }
            }

            return generationResult;
        }

        private IOrderedEnumerable<IGenomeInfo<T>> SelectFittest(IOrderedEnumerable<FitnessResult<T, TScore>> fitnessResults, int count)
        {
            return fitnessResults.Take(count).Select(r => r.GenomeInfo);
        }

        private IEnumerable<IGenomeInfo<T>> CreateGeneration(int count)
        {
            return ParallelEnumerable
                .Range(1, count)
                .Select(_ => new GenomeInfo<T>(_genomeFactory.GetNewGenome(), 0));
        }
    }
}