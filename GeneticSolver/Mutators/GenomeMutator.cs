using GeneticSolver.Expressions;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Mutators
{
    public class GenomeMutator<T> : MutatorBase<T>
    {
        private readonly IGenomeDescription<T> _genomeDescription;
        private readonly IValueSource<double> _mutationProbability;
        private readonly IRandom _random;

        public GenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability, IRandom random)
            : this(genomeDescription, StaticValueSource<double>.From(mutationProbability), random)
        {
        }

        public GenomeMutator(IGenomeDescription<T> genomeDescription, IValueSource<double> mutationProbability, IRandom random)
            : base(genomeDescription, mutationProbability)
        {
            _mutationProbability = mutationProbability;
            _genomeDescription = genomeDescription;
            _random = random;
        }

        protected override double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}