using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var evolver = new Evolver<Values>();
            ConsoleKeyInfo key = new ConsoleKeyInfo(' ', ConsoleKey.A, false, false, false);
            while (key.Key != ConsoleKey.X)
            {
                var best = evolver.Evolve(
                    new ValuesGenomeRandomGenerator(), new ValuesGenomeEvaluator(), 100, 1000);

                Console.WriteLine($"Best = {best.Value.Sum}");
                key = Console.ReadKey();
            }
        }
    }

    public class Values
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public int E { get; set; }

        public int Sum => A + B + C + D + E;
    }

    public class ValuesGenome : IGenome<Values>
    {
        public ValuesGenome(Values values)
        {
            Value = values;
        }
        public Values Value { get; }
    }

    public class ValuesGenomeEvaluator : IGenomeEvalautor<Values>
    {
        public int GetFitness(IGenome<Values> genome)
        {
            int sum = genome.Value.Sum;

            return IsPrime(sum) ? sum : sum / 4;
        }

        private static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i+=2)
                if (number % i == 0)
                    return false;

            return true;
        }
    }

    public class ValuesGenomeRandomGenerator : IGenomeRandomGenerator<Values>
    {
        private readonly Random _random = new Random();
        public IEnumerable<IGenome<Values>> CreateGeneration(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new ValuesGenome(new Values()
                {
                    A = _random.Next(0,1001),
                    B = _random.Next(0,1001),
                    C = _random.Next(0,1001),
                    D = _random.Next(0,1001),
                    E = _random.Next(0,1001),
                });
            }
        }

        public IEnumerable<IGenome<Values>> CreateChildren(int count, IGenome<Values> parentA, IGenome<Values> parentB)
        {
            for (int i = 0; i < count; i++)
            {
                double aPct = _random.NextDouble();
                double bPct = _random.NextDouble();
                double cPct = _random.NextDouble();
                double dPct = _random.NextDouble();
                double ePct = _random.NextDouble();

                yield return new ValuesGenome(new Values()
                {
                    A = (int)( parentA.Value.A * aPct + parentB.Value.A * (1 - aPct)),
                    B = (int)( parentA.Value.B * bPct + parentB.Value.B * (1 - bPct)),
                    C = (int)( parentA.Value.C * cPct + parentB.Value.C * (1 - cPct)),
                    D = (int)( parentA.Value.D * dPct + parentB.Value.D * (1 - dPct)),
                    E = (int)( parentA.Value.E * ePct + parentB.Value.A * (1 - ePct)),
                });
            }
        }

        public IEnumerable<IGenome<Values>> MutateGenomes(IEnumerable<IGenome<Values>> genomes)
        {
            foreach (var genome in genomes)
            {
                double aPct = _random.NextDouble();
                double bPct = _random.NextDouble();
                double cPct = _random.NextDouble();
                double dPct = _random.NextDouble();
                double ePct = _random.NextDouble();

                yield return new ValuesGenome(new Values()
                {
                    A = aPct > 0.95 ? AlterNumber(genome.Value.A) : genome.Value.A,
                    B = bPct > 0.95 ? AlterNumber(genome.Value.B) : genome.Value.B,
                    C = cPct > 0.95 ? AlterNumber(genome.Value.C) : genome.Value.C,
                    D = dPct > 0.95 ? AlterNumber(genome.Value.D) : genome.Value.D,
                    E = ePct > 0.95 ? AlterNumber(genome.Value.E) : genome.Value.E,
                });
            }
        }

        private int AlterNumber(int originalValue)
        {
            int modifier = _random.Next(0, 2) == 1 ? -1 : 1;
            int change = _random.Next(0, 20);
            int newValue = originalValue + modifier * change;
            if (newValue < 0) newValue = 0;
            if (newValue > 1000) newValue = 1000;
            return newValue;
        }
    }

    public class Evolver<T>
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

                // TODO: Produce random mutations through the next generation population.
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

    public interface IGenome<out T>
    {
        T Value { get; }
    }

    public interface IGenomeRandomGenerator<T>
    {
        IEnumerable<IGenome<T>> CreateGeneration(int count);
        IEnumerable<IGenome<T>> CreateChildren(int count, IGenome<T> parentA, IGenome<T> parentB);

        IEnumerable<IGenome<T>> MutateGenomes(IEnumerable<IGenome<T>> genomes);
    }

    public interface IGenomeEvalautor<T>
    {
        int GetFitness(IGenome<T> genome);
    }
}