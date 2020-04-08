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

    class GenomeInfo<T> : IGenomeInfo<T>
    {
        public GenomeInfo(T genome, int generation)
        {
            Genome = genome;
            Generation = generation;
        }

        public T Genome { get; }
        public int Generation { get; }
    }

    internal sealed class Generation<T, TScore> where TScore : IComparable<TScore>
    {
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeEvaluator<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly Random _random = new Random();

        public Generation(IEnumerable<IGenomeInfo<T>> genomes, IGenomeFactory<T> genomeFactory, IGenomeEvaluator<T, TScore> evaluator, IGenomeDescription<T> genomeDescription)
        {
            _genomeFactory = genomeFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            Genomes = genomes;
        }

        public IEnumerable<IGenomeInfo<T>> Genomes { get; }

        public IEnumerable<FitnessResult<T, TScore>> Score()
        {
            return _evaluator.GetFitnessResults(Genomes);
        }

        public Generation<T, TScore> Mutate()
        {
            return new Generation<T, TScore>(
                MutateGenomes(Genomes),
                _genomeFactory,
                _evaluator,
                _genomeDescription);
        }

        private IEnumerable<IGenomeInfo<T>> MutateGenomes(IEnumerable<IGenomeInfo<T>> genomes)
        {
            foreach (var genome in genomes)
            {
                foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() > 0.95))
                {
                    property.Mutate(genome.Genome);
                }

                yield return genome;
            }
        }

        public Generation<T, TScore> Concat(Generation<T, TScore> other)
        {
            return new Generation<T, TScore>(Genomes.Concat(other.Genomes).ToArray(), _genomeFactory, _evaluator, _genomeDescription);
        }

        public Generation<T, TScore> BreedPairs(int count, int generationNum)
        {
            return new Generation<T, TScore>(GetChildren(count, generationNum), _genomeFactory, _evaluator, _genomeDescription);
        }

        private IEnumerable<IGenomeInfo<T>> GetChildren(int count, int generationNum)
        {
            foreach (var pair in GetPairs(Genomes))
            {
                var children = CreateChildren(count, pair.Item1, pair.Item2, generationNum);
                var worthyChildren = GetFitnessResultsDescending(children)
                    .Take(2)
                    .Select(r => r.GenomeInfo)
                    .ToArray();

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

        private IOrderedEnumerable<FitnessResult<T, TScore>> GetFitnessResultsDescending(IEnumerable<IGenomeInfo<T>> generation)
        {
            return _evaluator.GetFitnessResults(generation).OrderByDescending(r => r.Fitness);
        }

        private IEnumerable<Tuple<IGenomeInfo<T>, IGenomeInfo<T>>> GetPairs(IEnumerable<IGenomeInfo<T>> genomes)
        {
            var genomesArr = genomes.ToArray();
            while (genomesArr.Length > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<IGenomeInfo<T>, IGenomeInfo<T>>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }
    }

    public interface ISolverLogger<T, TScore>
    {
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(ICollection<FitnessResult<T, TScore>> results);
        void LogGenome(FitnessResult<T, TScore> result);
    }

    public interface ISolverParameters
    {
        int MaxGenerationSize { get; }
    }

    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxGenerationSize)
        {
            MaxGenerationSize = maxGenerationSize;
        }

        public int MaxGenerationSize { get; }
    }


    public class Solver<T, TScore> where TScore : IComparable<TScore>
    {
        private readonly IGenomeFactory<T> _genomeFactory;
        private readonly IGenomeEvaluator<T, TScore> _evaluator;
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly ISolverLogger<T, TScore> _logger;
        private readonly ISolverParameters _solverParameters;
        private readonly Random _random = new Random();

        public Solver(IGenomeFactory<T> genomeFactory, IGenomeEvaluator<T, TScore> evaluator, IGenomeDescription<T> genomeDescription, ISolverLogger<T, TScore> logger, ISolverParameters solverParameters)
        {
            _genomeFactory = genomeFactory;
            _evaluator = evaluator;
            _genomeDescription = genomeDescription;
            _logger = logger;
            _solverParameters = solverParameters;
        }

        public T Evolve(int iterations, IEnumerable<T> originalGeneration = null)
        {
            int numberToKeep = HalfButEven(_solverParameters.MaxGenerationSize);

            var generation = _evaluator.GetFitnessResults(
         originalGeneration?.Select(g => new GenomeInfo<T>(g, 0)) 
                ?? CreateGeneration(_solverParameters.MaxGenerationSize)).ToArray();

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                _logger.LogStartGeneration(generationNum);

                var keepers = SelectFittest(generation, numberToKeep).ToArray();

                var children = GetChildren(keepers, 2, generationNum);
                children = MutateGenomes(children);

                var nextGenerationGenomes = keepers.Concat(children);

                generation = _evaluator.GetFitnessResults(nextGenerationGenomes).ToArray();

                _logger.LogGenerationInfo(generation);
                _logger.LogGenome(generation.First());
            }

            return SelectFittest(generation, 1).First().Genome;
        }

        private IEnumerable<Tuple<IGenomeInfo<T>, IGenomeInfo<T>>> GetPairs(IEnumerable<IGenomeInfo<T>> genomes)
        {
            var genomesArr = genomes.ToArray();
            while (genomesArr.Length > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<IGenomeInfo<T>, IGenomeInfo<T>>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }

        private IEnumerable<IGenomeInfo<T>> GetChildren(IEnumerable<IGenomeInfo<T>> genomes, int count, int generationNum)
        {
            foreach (var pair in GetPairs(genomes))
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

        private IEnumerable<IGenomeInfo<T>> MutateGenomes(IEnumerable<IGenomeInfo<T>> genomes)
        {
            foreach (var genome in genomes)
            {
                foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() > 0.95))
                {
                    property.Mutate(genome.Genome);
                }

                yield return genome;
            }
        }

        private IEnumerable<IGenomeInfo<T>> SelectFittest(IEnumerable<FitnessResult<T, TScore>> fitnessResults, int count)
        {
            return _evaluator.SortDescending(fitnessResults).Take(count).Select(r => r.GenomeInfo);
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

        private int HalfButEven(int value)
        {
            int half = value / 2;
            return half % 2 == 0 ? half : half - 1;
        }

    }
}