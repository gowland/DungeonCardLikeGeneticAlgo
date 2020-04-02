using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public class Solver<T>
    {
        public IGenome<T> Evolve(IGenomeRandomGenerator<T> genomeRandomGenerator, IGenomeEvalautor<T> evaluator, int count, int iterations)
        {
            // Produce an initial generation of Genomes using a random number generator.
            var generation = genomeRandomGenerator.CreateGeneration(count);

            for (int generationNum = 0; generationNum < iterations; generationNum++)
            {
                // Determine the fitness of all of the Genomes.
                // Determine which Genomes are allowed to reproduce.
                var worthyGenomes = generation
                    .OrderByDescending(evaluator.GetFitness)
                    .Take(HalfButEven(count)).ToArray();

                // Crossover  the Genome pairs  in the allowable population.
                // Pick the 2 fittest Genomes of the 2 parents and 2 children resulting from the crossover and add them       to the next generation.
                var children = GetChildren(genomeRandomGenerator, evaluator, count, worthyGenomes);

                // Produce random mutations through the next generation population.
                children = genomeRandomGenerator.MutateGenomes(children);

                // Calculate the next generations fitness and loop back to step 3.
                generation = worthyGenomes.Concat(children).ToArray();
            }

            return generation.OrderByDescending(evaluator.GetFitness).First();
        }

        private IEnumerable<IGenome<T>> GetChildren(IGenomeRandomGenerator<T> genomeRandomGenerator, IGenomeEvalautor<T> evalautor, int count,
            IEnumerable<IGenome<T>> worthyGenomes)
        {
            foreach (var pair in GetPairs(worthyGenomes))
            {
                var children = genomeRandomGenerator.CreateChildren(count, pair.Item1, pair.Item2);
                var worthyChildren = children.OrderByDescending(evalautor.GetFitness).Take(2).ToArray();
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
            while (genomesArr.Count() > 1)
            {
                var pair = genomesArr.Take(2).ToArray();
                yield return new Tuple<IGenome<T>, IGenome<T>>(pair[0], pair[1]);

                genomesArr = genomesArr.Skip(2).ToArray();
            }
        }
    }
}