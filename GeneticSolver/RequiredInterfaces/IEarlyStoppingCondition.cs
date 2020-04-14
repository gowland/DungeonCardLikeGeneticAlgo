using System.Linq;

namespace GeneticSolver.RequiredInterfaces
{
    public interface IEarlyStoppingCondition<T, TScore>
    {
        bool Match(int generationNumber, IOrderedEnumerable<FitnessResult<T, TScore>> generation);
    }
}