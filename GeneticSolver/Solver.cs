using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public class Generation<T>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private readonly IGenomeEvalautor<T> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;

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
                _generationFactory.MutateGenomes(Genomes),
                _generationFactory,
                _evaluator,
                _genomeDescription);
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
                var children = _generationFactory.CreateChildren(count, pair.Item1, pair.Item2);
                var worthyChildren = GetFitnessResultsDescending(children)
                    .Take(2)
                    .Select(r => r.Genome)
                    .ToArray();

                yield return worthyChildren[0];
                yield return worthyChildren[1];
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

    public class ScoredGeneration<T>
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
            var generation = new Generation<T>(_generationFactory.CreateGeneration(count), _generationFactory, _evaluator, _genomeDescription);

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                Console.WriteLine($"Starting generation {generationNum}");
                var scoredGeneration = generation.Score();
                var keepers = scoredGeneration.TakeBest(numberToKeep);
                var children = keepers.BreedPairs(2).Mutate();
                generation = keepers.Concat(children);
            }

            return generation.Score().TakeBest(1).Genomes.First();

/*
            // Produce an initial generation of Genomes using a random number generator.
            var generation = _generationFactory.CreateGeneration(count);

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                // Determine the fitness of all of the Genomes.
                // Determine which Genomes are allowed to reproduce.
                var worthyGenomes = GetFitnessResultsDescending(generation)
                    .TakeBest(HalfButEven(count))
                    .Select(r => r.Genome)
                    .ToArray();

                // Crossover  the Genome pairs  in the allowable population.
                // Pick the 2 fittest Genomes of the 2 parents and 2 children resulting from the crossover and add them to the next generation.
                var children = GetChildren(count, worthyGenomes);

                // Produce random mutations through the next generation population.
                children = _generationFactory.MutateGenomes(children);

                // Concat the current generation's best genomes to the new set of child genomes to produce a new generation
                generation = worthyGenomes.Concat(children).ToArray();
            }

            return GetFitnessResultsDescending(generation).First().Genome;
*/
        }

        private IOrderedEnumerable<FitnessResult<T>> GetFitnessResultsDescending(IEnumerable<T> generation)
        {
            return _evaluator.GetFitnessResults(generation).OrderByDescending(r => r.Fitness);
        }

        private IEnumerable<T> GetChildren(int count, IEnumerable<T> worthyGenomes)
        {
            foreach (var pair in GetPairs(worthyGenomes))
            {
                var children = _generationFactory.CreateChildren(count, pair.Item1, pair.Item2);
                var worthyChildren = GetFitnessResultsDescending(children)
                    .Take(2)
                    .Select(r => r.Genome)
                    .ToArray();

                yield return worthyChildren[0];
                yield return worthyChildren[1];
            }
        }

        private int HalfButEven(int value)
        {
            int half = value / 2;
            return half % 2 == 0 ? half : half - 1;
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
}