using System.Linq;
using GeneticSolver.Expressions;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Mutators
{
    public class GenomeMutator<T> : IMutator<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly IValueSource<double> _mutationProbability;
        private readonly IRandom _random;

        public GenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability, IRandom random)
            : this(genomeDescription, StaticValueSource<double>.From(mutationProbability), random)
        {
        }

        public GenomeMutator(IGenomeDescription<T> genomeDescription, IValueSource<double> mutationProbability, IRandom random)
        {
            _mutationProbability = mutationProbability;
            _genomeDescription = genomeDescription;
            _random = random;
        }

        public void Mutate(T genome)
        {
            foreach (var property in _genomeDescription.Properties.Where(p => _random.NextDouble() < _mutationProbability.GetValue()))
            {
                property.Mutate(genome);
            }
        }
    }
}