using System;
using System.Linq;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver
{
    public class GenomeMutator<T> : IMutator<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly double _mutationProbability;
        private readonly IRandom _random;

        public GenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability, IRandom random)
        {
            _genomeDescription = genomeDescription;
            _mutationProbability = mutationProbability;
            _random = random;
        }

        public void Mutate(T genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() < _mutationProbability))
            {
                property.Mutate(genome);
            }
        }
    }
}