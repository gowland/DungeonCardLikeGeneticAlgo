using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public class Solver<T>
    {
        private readonly IGenerationFactory<T> _generationFactory;
        private IGenomeEvalautor<T> _evaluator;

        public Solver(IGenerationFactory<T> generationFactory, IGenomeEvalautor<T> evaluator)
        {
            _generationFactory = generationFactory;
            _evaluator = evaluator;
        }

        public IGenome<T> Solve(int count, int iterations)
        {
            // Produce an initial generation of Genomes using a random number generator.
            var generation = _generationFactory.CreateGeneration(count);

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                // Determine the fitness of all of the Genomes.
                // Determine which Genomes are allowed to reproduce.
                var worthyGenomes = GetFitnessResultsDescending(generation)
                    .Take(HalfButEven(count))
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
        }

        private IOrderedEnumerable<FitnessResult<T>> GetFitnessResultsDescending(IEnumerable<IGenome<T>> generation)
        {
            return _evaluator.GetFitnessResults(generation).OrderByDescending(r => r.Fitness);
        }

        private IEnumerable<IGenome<T>> GetChildren(int count, IEnumerable<IGenome<T>> worthyGenomes)
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

        private IEnumerable<Tuple<IGenome<T>, IGenome<T>>> GetPairs(IEnumerable<IGenome<T>> genomes)
        {
            var genomesArr = genomes.ToArray();
            while (genomesArr.Length > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<IGenome<T>, IGenome<T>>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }
    }
}