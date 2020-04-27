using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Genome;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public interface IMutator<in T>
    {
        void Mutate(T genome);
    }

    public class GenomeMutator<T> : IMutator<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly double _mutationProbability;
        private readonly Random _random;

        public GenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability, Random random = null)
        {
            _genomeDescription = genomeDescription;
            _mutationProbability = mutationProbability;
            _random = random ?? new Random();
        }

        public void Mutate(T genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() < _mutationProbability))
            {
                property.Mutate(genome);
            }
        }
    }

    public interface IGenomeReproductionStrategy<T>
    {
        IEnumerable<T> ProduceOffspring(IEnumerable<T> parents);
    }

    public class AsexualGenomeReproductionStrategy<T> : IGenomeReproductionStrategy<T>
        where T : class, ICloneable 
    {
        private readonly IMutator<T> _mutator;

        public AsexualGenomeReproductionStrategy(IMutator<T> mutator)
        {
            _mutator = mutator;
        }

        public IEnumerable<T> ProduceOffspring(IEnumerable<T> parents)
        {
            var nextGen = parents
                .Select(genome => genome.Clone() as T)
                .ToList();

            nextGen.ForEach(_mutator.Mutate);

            return nextGen;
        }
    }

    public interface IPairingStrategy<T>
    {
        IEnumerable<Tuple<T, T>> GetPairs(IEnumerable<T> items);
    }

    public class SexualGenomeReproductionStrategy<T, TScore> : IGenomeReproductionStrategy<T>
        where T : class, ICloneable 
        where TScore : IComparable<TScore>
    {
        private readonly IMutator<T> _mutator;
        private readonly IPairingStrategy _pairingStrategy;
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly IGenomeEvaluator<T, TScore> _genomeEvaluator;
        private readonly int _childrenToCreate;
        private readonly int _childrenToKeepPerPair;

        public SexualGenomeReproductionStrategy(
            IMutator<T> mutator,
            IPairingStrategy pairingStrategy,
            IGenomeFactory<T> genomeFactory,
            IGenomeDescription<T> genomeDescription,
            IGenomeEvaluator<T, TScore> genomeEvaluator,
            int childrenToCreate,
            int childrenToKeepPerPair)
        {
            _mutator = mutator;
            _pairingStrategy = pairingStrategy;
            _genomeFactory = genomeFactory;
            _genomeDescription = genomeDescription;
            _genomeEvaluator = genomeEvaluator;
            _childrenToCreate = childrenToCreate;
            _childrenToKeepPerPair = childrenToKeepPerPair;
        }

        public IEnumerable<T> ProduceOffspring(IEnumerable<T> parents)
        {
            var nextGen = _pairingStrategy.GetPairs(parents)
                .Select(pair => CreateChildren(pair.Item1, pair.Item2))
                .SelectMany(TakeFittest)
                .ToList();

            nextGen.ForEach(_mutator.Mutate);

            return nextGen;
        }

        private IEnumerable<T> CreateChildren(T parentA, T parentB)
        {
            for (int i = 0; i < _childrenToCreate; i++)
            {
                var child = _genomeFactory.GetNewGenome();

                foreach (var property in _genomeDescription.Properties)
                {
                    property.Merge(parentA, parentB, child);
                }

                yield return child;
            }
        }

        private IEnumerable<T> TakeFittest(IEnumerable<T> genomes)
        {
            return _genomeEvaluator
                .GetFitnessResults(genomes)
                .Take(_childrenToKeepPerPair);
        }
    }

    public class Solver<T, TScore> 
        where T : ICloneable
        where TScore : IComparable<TScore>
    {
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeEvaluator<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly ISolverLogger<T, TScore> _logger;
        private readonly ISolverParameters _solverParameters;
        private readonly IEnumerable<IEarlyStoppingCondition<T, TScore>> _earlyStoppingConditions;
        private readonly IEnumerable<IGenomeReproductionStrategy<T>> _genomeReproductionStrategies;
        private readonly Random _random = new Random();

        public Solver(IGenomeFactory<T> genomeFactory,
            IGenomeEvaluator<T, TScore> evaluator,
            IGenomeDescription<T> genomeDescription, 
            ISolverLogger<T, TScore> logger, ISolverParameters solverParameters, 
            IEnumerable<IEarlyStoppingCondition<T, TScore>> earlyStoppingConditions,
            IEnumerable<IGenomeReproductionStrategy<T>> genomeReproductionStrategies)
        {
            _genomeFactory = genomeFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
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

/*
                var children = GetChildren(elite, 2, generationNum).ToArray();
                children.ToList().ForEach(MutateGenome);
*/
                var num = generationNum;
                var children = _genomeReproductionStrategies
                    .SelectMany(reproductionStrategy =>
                        reproductionStrategy.ProduceOffspring(elite.Select(g => g.Genome)))
                    .Select(g => new GenomeInfo<T>(g, num));

                if (_solverParameters.MutateParents)
                {
                    // Do not mutate the fittest genome
                    elite.Skip(1).ToList().ForEach(MutateGenome);
                }

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

        private void MutateGenome(IGenomeInfo<T> genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p =>
                _random.NextDouble() < _solverParameters.PropertyMutationProbability))
            {
                property.Mutate(genome.Genome);
            }
        }

        private IOrderedEnumerable<IGenomeInfo<T>> SelectFittest(IOrderedEnumerable<FitnessResult<T, TScore>> fitnessResults, int count)
        {
            return fitnessResults.Take(count).Select(r => r.GenomeInfo);
        }

        private IEnumerable<IGenomeInfo<T>> CreateGeneration(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var genome = _genomeFactory.GetNewGenome();
                foreach (var property in _genomeDescription.Properties)
                {
                    property.SetRandom(genome);
                }

                yield return new GenomeInfo<T>(genome, 0);
            }
        }
    }

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