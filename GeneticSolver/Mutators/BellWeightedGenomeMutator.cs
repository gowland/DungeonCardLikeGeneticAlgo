using System.Collections.Generic;
using System.Linq;
using GeneticSolver.Expressions;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.Mutators
{
    public class BellWeightedGenomeMutator<T> : MutatorBase<T>
    {
        private static readonly double[] _stdDeviationsCycle = {10, 1, 0.1, 0.01, 0.001, 0.0001};
        private Queue<double> currentQueue = new Queue<double>(_stdDeviationsCycle);
        private Queue<double> usedValueQueue = new Queue<double>();
        private IRandom _random;

        public BellWeightedGenomeMutator(IGenomeDescription<T> genomeDescription, double mutationProbability)
            : base(genomeDescription, new StaticValueSource<double>(mutationProbability))
        {
            _random = new BellWeightedRandom(1);
        }

        public void CycleStdDev()
        {
            if (currentQueue.Count <= 0)
            {
                var tmp = currentQueue;
                currentQueue = usedValueQueue;
                usedValueQueue = tmp;
            }

            double currentStdDev = currentQueue.Dequeue();
            usedValueQueue.Enqueue(currentStdDev);
            _random = new BellWeightedRandom(currentStdDev);
        }

        protected override double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}