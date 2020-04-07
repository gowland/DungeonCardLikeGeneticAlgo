using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    internal sealed class Generation<T>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly Random _random = new Random();

        public Generation(IEnumerable<T> genomes, IGenerationFactory<T> generationFactory, IGenomeEvalautor<T> evaluator, IGenomeDescription<T> genomeDescription)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            Genomes = genomes;
        }

        public IEnumerable<T> Genomes { get; }

        public ScoredGeneration<T> Score()
        {
            return new ScoredGeneration<T>(_evaluator.GetFitnessResults(Genomes), _generationFactory, _evaluator, _genomeDescription);
        }

        public Generation<T> Mutate()
        {
            return new Generation<T>(
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

        public Generation<T> Concat(Generation<T> other)
        {
            return new Generation<T>(Genomes.Concat(other.Genomes).ToArray(), _generationFactory, _evaluator, _genomeDescription);
        }

        public Generation<T> BreedPairs(int count)
        {
            return new Generation<T>(GetChildren(count), _generationFactory, _evaluator, _genomeDescription);
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

        private IOrderedEnumerable<FitnessResult<T>> GetFitnessResultsDescending(IEnumerable<T> generation)
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

    internal sealed class ScoredGeneration<T>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;

        public ScoredGeneration(IEnumerable<FitnessResult<T>> fitnessResults, IGenerationFactory<T> generationFactory, IGenomeEvalautor<T> evaluator, IGenomeDescription<T> genomeDescription)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            FitnessResults = fitnessResults;
        }

        public IEnumerable<FitnessResult<T>> FitnessResults { get; }
        public IEnumerable<T> Genomes => FitnessResults.Select(r => r.Genome);

        public Generation<T> TakeBest(int count)
        {
            return new Generation<T>(
                FitnessResults.OrderByDescending(r => r.Fitness)
                    .Take(count)
                    .Select(r => r.Genome),
                _generationFactory,
                _evaluator,
                _genomeDescription);
        }
    }

    public class Solver<T>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;

        public Solver(IGenerationFactory<T> generationFactory, IGenomeEvalautor<T> evaluator, IGenomeDescription<T> genomeDescription)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
        }

        public T Solve(int count, int iterations)
        {
            int numberToKeep = HalfButEven(count);
            var generation = new Generation<T>(CreateGeneration(count), _generationFactory, _evaluator, _genomeDescription);

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                Console.WriteLine($"Starting generation {generationNum}");
                var scoredGeneration = generation.Score();
                var keepers = scoredGeneration.TakeBest(numberToKeep);
                var children = keepers.BreedPairs(2).Mutate();
                generation = keepers.Concat(children);
            }

            return generation.Score().TakeBest(1).Genomes.First();
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

        private IOrderedEnumerable<FitnessResult<T>> GetFitnessResultsDescending(IEnumerable<T> generation)
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