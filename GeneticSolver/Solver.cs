using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Genome;
using GeneticSolver.Interfaces;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class Solver<T, TScore> where TScore : IComparable<TScore>
    {
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeEvaluator<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly ISolverLogger<T, TScore> _logger;
        private readonly ISolverParameters _solverParameters;
        private readonly IEnumerable<IEarlyStoppingCondition<T, TScore>> _earlyStoppingConditions;
        private readonly Random _random = new Random();

        public Solver(IGenomeFactory<T> genomeFactory, IGenomeEvaluator<T, TScore> evaluator, IGenomeDescription<T> genomeDescription, ISolverLogger<T, TScore> logger, ISolverParameters solverParameters, IEnumerable<IEarlyStoppingCondition<T, TScore>> earlyStoppingConditions)
        {
            _genomeFactory = genomeFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            _logger = logger;
            _solverParameters = solverParameters;
            _earlyStoppingConditions = earlyStoppingConditions;
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

                IOrderedEnumerable<IGenomeInfo<T>> keepers = SelectFittest(generation, _solverParameters.MaxEliteSize);

                var children = GetChildren(keepers, 2, generationNum).ToArray();
                children.ToList().ForEach(MutateGenome);

                if (_solverParameters.MutateParents)
                {
                    // Do not mutate the fittest genome
                    keepers.Skip(1).ToList().ForEach(MutateGenome);
                }

                var nextGenerationGenomes = keepers.Concat(children).ToArray();

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

        private IEnumerable<IGenomeInfo<T>> GetChildren(IOrderedEnumerable<IGenomeInfo<T>> genomes, int count, int generationNum)
        {
            foreach (var pair in _solverParameters.BreadingStrategy.GetPairs(genomes))
            {
                var children = CreateChildren(count, pair.Item1, pair.Item2, generationNum);
                var worthyChildren = SelectFittest(_evaluator.GetFitnessResults(children), 2).ToArray();

                yield return worthyChildren[0];
                yield return worthyChildren[1];
            }
        }

        public IEnumerable<IGenomeInfo<T>> CreateChildren(int count, IGenomeInfo<T> parentA, IGenomeInfo<T> parentB, int generationNum)
        {
            for (int i = 0; i < count; i++)
            {
                var child = _genomeFactory.GetNewGenome();

                foreach (var property in _genomeDescription.Properties)
                {
                    property.Merge(parentA.Genome, parentB.Genome, child);
                }

                yield return new GenomeInfo<T>(child, generationNum);
            }
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