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
        public bool Match(GenerationResult<T, double> generationResult)
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

    public class ProgressStalledEarlyStoppingCondition<T, TScore> : IEarlyStoppingCondition<T, TScore>
    {
        private readonly int _minGenerationsToTakeEffect;
        private readonly double _mostFitGenomeMaxAgePercentage;
        private readonly double _averageGenomeMaxAgePercentage;

        public ProgressStalledEarlyStoppingCondition(int minGenerationsToTakeEffect, double mostFitGenomeMaxAgePercentage, double averageGenomeMaxAgePercentage)
        {
            _minGenerationsToTakeEffect = minGenerationsToTakeEffect;
            _mostFitGenomeMaxAgePercentage = mostFitGenomeMaxAgePercentage;
            _averageGenomeMaxAgePercentage = averageGenomeMaxAgePercentage;
        }

        public bool Match(GenerationResult<T, TScore> generationResult)
        {
            int mostFitGenomeGeneration = generationResult.FittestGenome.GenomeInfo.Generation;
            double averageGenomeGeneration = generationResult.AverageGenomeGeneration;

            return generationResult.GenerationNumber > _minGenerationsToTakeEffect 
                   && (mostFitGenomeGeneration / (double)generationResult.GenerationNumber < _mostFitGenomeMaxAgePercentage || averageGenomeGeneration / generationResult.GenerationNumber < _averageGenomeMaxAgePercentage);
        }
    }
}