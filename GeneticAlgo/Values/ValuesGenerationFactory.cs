using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;

namespace GeneticAlgo.Values
{
    public class ValuesGenerationFactory : IGenerationFactory<Values>
    {
        private readonly IGenomeDescription<Values> _genomeDescription;
        private readonly Random _random = new Random();

        public ValuesGenerationFactory(IGenomeDescription<Values> genomeDescription)
        {
            _genomeDescription = genomeDescription;
        }

        public Values GetNewGenome()
        {
            return new Values();
        }

        public IEnumerable<Values> CreateGeneration(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var genome = GetNewGenome();
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
                var child = GetNewGenome();

                foreach (var property in _genomeDescription.Properties)
                {
                    property.Merge(parentA, parentB, child);
                }

                yield return child;
            }
        }
    }
}