using System.Collections.Generic;
using System.Linq;

namespace GeneticSolver
{
    public interface ISolverLogger<T, TScore>
    {
        void Start(ISolverParameters parameters);
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(int generationNumber, IOrderedEnumerable<FitnessResult<T, TScore>> results);
        void End();
    }
}