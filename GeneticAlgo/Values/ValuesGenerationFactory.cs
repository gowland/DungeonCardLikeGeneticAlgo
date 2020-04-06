using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenerationFactory : IGenerationFactory<Values>
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
                foreach (var property in genome.Properties.Where(p => _random.NextDouble() > 0.95))
                {
                    property.Mutate();
                }

                yield return genome;

/*
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
*/
            }
        }

        private int AlterNumber(int originalValue)
        {
            int modifier = _random.Next(0, 2) == 1 ? -1 : 1;
            int change = _random.Next(0, 30);
            int newValue = originalValue + modifier * change;
            if (newValue < 0) newValue = 0;
            if (newValue > 1000) newValue = 1000;
            return newValue;
        }
    }
}