using System.Linq;
using GeneticSolver.Expressions;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Mutators
{
    public abstract class MutatorBase<T> : IMutator<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly IValueSource<double> _mutationProbability;

        protected MutatorBase(IGenomeDescription<T> genomeDescription, IValueSource<double> mutationProbability)
        {
            _mutationProbability = mutationProbability;
            _genomeDescription = genomeDescription;
        }

        public void Mutate(T genome)
        {
            var propertiesToMutate = _genomeDescription.Properties
                .Where(p => NextDouble() < _mutationProbability.GetValue())
                .ToList();

            if (!propertiesToMutate.Any())
            {
                propertiesToMutate.Add(_genomeDescription.Properties.OrderBy(p => NextDouble()).First());
            }

            foreach (var property in propertiesToMutate)
            {
                property.Mutate(genome);
            }
        }

        protected abstract double NextDouble();
    }
}