using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public interface IGenomeInfo<T>
    {
        T Genome { get; }
        int Generation { get; }
    }

    internal sealed class Generation<T, TScore> where TScore : IComparable<TScore>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly Random _random = new Random();

        public Generation(IEnumerable<T> genomes, IGenerationFactory<T> generationFactory, IGenomeEvalautor<T, TScore> evaluator, IGenomeDescription<T> genomeDescription)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            Genomes = genomes;
        }

        public IEnumerable<T> Genomes { get; }

        public ScoredGeneration<T, TScore> Score()
        {
            return new ScoredGeneration<T, TScore>(_evaluator.GetFitnessResults(Genomes), _generationFactory, _evaluator, _genomeDescription);
        }

        public Generation<T, TScore> Mutate()
        {
            return new Generation<T, TScore>(
                MutateGenomes(Genomes),
                _generationFactory,
                _evaluator,
                _genomeDescription);
        }

        private IEnumerable<T> MutateGenomes(IEnumerable<T> genomes)
        {
            foreach (var genome in genomes)
            {
                foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() > 0.95))
                {
                    property.Mutate(genome);
                }

                yield return genome;
            }
        }

        public Generation<T, TScore> Concat(Generation<T, TScore> other)
        {
            return new Generation<T, TScore>(Genomes.Concat(other.Genomes).ToArray(), _generationFactory, _evaluator, _genomeDescription);
        }

        public Generation<T, TScore> BreedPairs(int count)
        {
            return new Generation<T, TScore>(GetChildren(count), _generationFactory, _evaluator, _genomeDescription);
        }

        private IEnumerable<T> GetChildren(int count)
        {
            foreach (var pair in GetPairs(Genomes))
            {
                var children = CreateChildren(count, pair.Item1, pair.Item2);
                var worthyChildren = GetFitnessResultsDescending(children)
                    .Take(2)
                    .Select(r => r.Genome)
                    .ToArray();

                yield return worthyChildren[0];
                yield return worthyChildren[1];
            }
        }

        public IEnumerable<T> CreateChildren(int count, T parentA, T parentB)
        {
            for (int i = 0; i < count; i++)
            {
                var child = _generationFactory.GetNewGenome();

                foreach (var property in _genomeDescription.Properties)
                {
                    property.Merge(parentA, parentB, child);
                }

                yield return child;
            }
        }

        private IOrderedEnumerable<FitnessResult<T, TScore>> GetFitnessResultsDescending(IEnumerable<T> generation)
        {
            return _evaluator.GetFitnessResults(generation).OrderByDescending(r => r.Fitness);
        }

        private IEnumerable<Tuple<T, T>> GetPairs(IEnumerable<T> genomes)
        {
            var genomesArr = genomes.ToArray();
            while (genomesArr.Length > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<T, T>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }
    }

    internal sealed class ScoredGeneration<T, TScore> where TScore : IComparable<TScore>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;

        public ScoredGeneration(IEnumerable<FitnessResult<T, TScore>> fitnessResults, IGenerationFactory<T> generationFactory, IGenomeEvalautor<T, TScore> evaluator, IGenomeDescription<T> genomeDescription)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            FitnessResults = fitnessResults;
        }

        public IEnumerable<FitnessResult<T, TScore>> FitnessResults { get; }
        public IEnumerable<T> Genomes => FitnessResults.Select(r => r.Genome);

        public Generation<T, TScore> TakeBest(int count)
        {
            return new Generation<T, TScore>(
                FitnessResults.OrderByDescending(r => r.Fitness)
                    .Take(count)
                    .Select(r => r.Genome),
                _generationFactory,
                _evaluator,
                _genomeDescription);
        }
    }

    public interface ISolverLogger<T, TScore>
    {
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(IEnumerable<FitnessResult<T, TScore>> results);
    }

    public class Solver<T, TScore> where TScore : IComparable<TScore>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly ISolverLogger<T, TScore> _logger;

        public Solver(IGenerationFactory<T> generationFactory, IGenomeEvalautor<T, TScore> evaluator, IGenomeDescription<T> genomeDescription, ISolverLogger<T, TScore> logger)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            _logger = logger;
        }

        public T Solve(int count, int iterations)
        {
            int numberToKeep = HalfButEven(count);
            var generation = new Generation<T, TScore>(CreateGeneration(count), _generationFactory, _evaluator, _genomeDescription).Score();

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                _logger.LogStartGeneration(generationNum);
                _logger.LogGenerationInfo(generation.FitnessResults);
                var keepers = generation.TakeBest(numberToKeep);
                var children = keepers.BreedPairs(2).Mutate();
                generation = keepers.Concat(children).Score();
            }

            return generation.TakeBest(1).Genomes.First();
        }

        private IEnumerable<T> CreateGeneration(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var genome = _generationFactory.GetNewGenome();
                foreach (var property in _genomeDescription.Properties)
                {
                    property.SetRandom(genome);
                }

                yield return genome;
            }
        }

        private IOrderedEnumerable<FitnessResult<T, TScore>> GetFitnessResultsDescending(IEnumerable<T> generation)
        {
            return _evaluator.GetFitnessResults(generation).OrderByDescending(r => r.Fitness);
        }

        private int HalfButEven(int value)
        {
            int half = value / 2;
            return half % 2 == 0 ? half : half - 1;
        }
    }
}