using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
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

                if (_solverParameters.MutateParents)
                {
                    keepers = MutateGenomes(keepers).ToArray();
                }

                var nextGenerationGenomes = keepers.Concat(children);

                generation = _evaluator.GetFitnessResults(nextGenerationGenomes).ToArray();

                _logger.LogGenerationInfo(generation);
                _logger.LogGenome(generation.First());
            }

            return SelectFittest(generation, 1).First().Genome;
        }

        private IEnumerable<Tuple<IGenomeInfo<T>, IGenomeInfo<T>>> GetPairs(IEnumerable<IGenomeInfo<T>> genomes)
        {
            var genomesArr = _solverParameters.RandomizeMating ? genomes.OrderBy(g => _random.NextDouble()).ToArray() : genomes.ToArray();

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