using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public interface ISolverLogger<T, TScore>
    {
        void Start();
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(IOrderedEnumerable<FitnessResult<T, TScore>> results);
        void End();
    }
}