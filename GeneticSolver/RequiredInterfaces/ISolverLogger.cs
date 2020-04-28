namespace GeneticSolver.RequiredInterfaces
{
    public interface ISolverLogger<T, TScore>
    {
        void Start();
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(GenerationResult<T, TScore> generationResult);
        void LogGeneration(GenerationResult<T, TScore> generation);
        void End();
    }
}