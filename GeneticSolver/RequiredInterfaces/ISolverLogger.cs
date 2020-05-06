namespace GeneticSolver.RequiredInterfaces
{
    public interface ISolverLogger<T, TScore>
    {
        void Start();
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(IGenerationResult<T, TScore> generationResult);
        void LogGeneration(IGenerationResult<T, TScore> generation);
        void End();
    }
}