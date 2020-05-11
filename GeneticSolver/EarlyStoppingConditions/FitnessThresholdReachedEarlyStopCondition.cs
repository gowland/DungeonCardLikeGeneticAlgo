using System;
using System.Linq;
using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.EarlyStoppingConditions
{
    public class FitnessThresholdReachedEarlyStopCondition <T, TScore>: IEarlyStoppingCondition<T, TScore>
    {
        private readonly Func<TScore, bool> _fitnessScoreConditionFunc;

        public FitnessThresholdReachedEarlyStopCondition(Func<TScore, bool> fitnessScoreConditionFunc)
        {
            _fitnessScoreConditionFunc = fitnessScoreConditionFunc;
        }

        public bool Match(IGenerationResult<T, TScore> generationResult)
        {
            return _fitnessScoreConditionFunc(generationResult.FittestGenome.Fitness);
        }
    }
}