using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenerationFactory : IGenerationFactory<Values>
    {
        private readonly IGenome<Values> _genomeDescription;
        private readonly Random _random = new Random();

        public ValuesGenerationFactory(IGenome<Values> genomeDescription)
        {
            _genomeDescription = genomeDescription;
        }

        public IEnumerable<Values> CreateGeneration(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var genome = new Values();
                foreach (var property in _genomeDescription.Properties)
                {
                    property.SetRandom(genome);
                }

                yield return genome;
            }
        }

        public IEnumerable<Values> CreateChildren(int count, Values parentA, Values parentB)
        {
            for (int i = 0; i < count; i++)
            {
                double aPct = _random.NextDouble();
                double bPct = _random.NextDouble();
                double cPct = _random.NextDouble();
                double dPct = _random.NextDouble();
                double ePct = _random.NextDouble();

                yield return new Values()
                {
                    A = (int)( parentA.A * aPct + parentB.A * (1 - aPct)),
                    B = (int)( parentA.B * bPct + parentB.B * (1 - bPct)),
                    C = (int)( parentA.C * cPct + parentB.C * (1 - cPct)),
                    D = (int)( parentA.D * dPct + parentB.D * (1 - dPct)),
                    E = (int)( parentA.E * ePct + parentB.A * (1 - ePct)),
                };
            }
        }

        public IEnumerable<Values> MutateGenomes(IEnumerable<Values> genomes)
        {
            foreach (var genome in genomes)
            {
                foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() > 0.95))
                {
                    property.Mutate(genome);
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