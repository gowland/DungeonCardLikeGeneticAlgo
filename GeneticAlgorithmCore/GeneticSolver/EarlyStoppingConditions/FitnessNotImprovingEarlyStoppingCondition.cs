using System;
using System.Collections.Generic;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.EarlyStoppingConditions
{
    public class FitnessNotImprovingEarlyStoppingCondition<T> : IEarlyStoppingCondition<T, double>
    {
        private readonly double _minImprovement;
        private readonly int _numGenerations;
        private readonly Queue<double> _previousGenerationFitnesses;

        public FitnessNotImprovingEarlyStoppingCondition(double minImprovement, int numGenerations)
        {
            _minImprovement = minImprovement;
            _numGenerations = numGenerations;
            _previousGenerationFitnesses = new Queue<double>();
        }
        public bool Match(IGenerationResult<T, double> generationResult)
        {
            double currentGenerationFitness = generationResult.FittestGenome.Fitness;
            _previousGenerationFitnesses.Enqueue(currentGenerationFitness);

            if (_previousGenerationFitnesses.Count > _numGenerations)
            {
                double oldestGenerationFitness = _previousGenerationFitnesses.Dequeue();
                if (Math.Abs(oldestGenerationFitness - currentGenerationFitness) < _minImprovement)
                {
                    return true;
                }
            }

            return false;
        }
    }
}