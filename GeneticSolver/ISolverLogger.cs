using System.Collections.Generic;

namespace GeneticSolver
{
    public interface ISolverLogger<T, TScore>
    {
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(ICollection<FitnessResult<T, TScore>> results);
        void LogGenome(FitnessResult<T, TScore> result);
    }
}