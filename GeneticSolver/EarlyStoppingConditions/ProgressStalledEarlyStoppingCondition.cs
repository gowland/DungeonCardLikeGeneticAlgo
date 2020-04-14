using System.Linq;
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

        public bool Match(int generationNumber, IOrderedEnumerable<FitnessResult<T, TScore>> generation)
        {
            int mostFitGenomeGeneration = generation.First().GenomeInfo.Generation;
            double averageGenomeGeneration = generation.Average(g => g.GenomeInfo.Generation);

            return generationNumber > _minGenerationsToTakeEffect 
                   && (mostFitGenomeGeneration / (double)generationNumber < _mostFitGenomeMaxAgePercentage || averageGenomeGeneration / generationNumber < _averageGenomeMaxAgePercentage);
        }
    }
}