using GeneticSolver.RequiredInterfaces;

namespace GeneticSolver.EarlyStoppingConditions
{
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

        public bool Match(IGenerationResult<T, TScore> generationResult)
        {
            int mostFitGenomeGeneration = generationResult.FittestGenome.GenomeInfo.Generation;
            double averageGenomeGeneration = generationResult.AverageGenomeGeneration;

            return generationResult.GenerationNumber > _minGenerationsToTakeEffect 
                   && (mostFitGenomeGeneration / (double)generationResult.GenerationNumber < _mostFitGenomeMaxAgePercentage || averageGenomeGeneration / generationResult.GenerationNumber < _averageGenomeMaxAgePercentage);
        }
    }
}