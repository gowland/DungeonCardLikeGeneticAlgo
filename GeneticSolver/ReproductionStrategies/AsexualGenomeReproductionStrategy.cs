using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.ReproductionStrategies
{
    public class AsexualGenomeReproductionStrategy<T> : IGenomeReproductionStrategy<T>
        where T : class, ICloneable 
    {
        private readonly IMutator<T> _mutator;

        public AsexualGenomeReproductionStrategy(IMutator<T> mutator)
        {
            _mutator = mutator;
        }

        public IEnumerable<T> ProduceOffspring(IEnumerable<T> parents)
        {
            var nextGen = parents
                .Select(genome => genome.Clone() as T)
                .ToArray();

            foreach (var genome in nextGen)
            {
                _mutator.Mutate(genome);
            }

            return nextGen;
        }
    }
}