using System.Linq;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IEarlyStoppingCondition<T, TScore>
    {
        bool Match(IGenerationResult<T, TScore> generationResult);
    }
}