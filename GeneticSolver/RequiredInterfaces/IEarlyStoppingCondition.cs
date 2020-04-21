using System.Linq;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IEarlyStoppingCondition<T, TScore>
    {
        bool Match(GenerationResult<T, TScore> generationResult);
    }
}