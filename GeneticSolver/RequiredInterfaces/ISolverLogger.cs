namespace GeneticSolver.RequiredInterfaces
{
    public interface ISolverLogger<T, TScore>
    {
        void Start(ISolverParameters parameters);
        void LogStartGeneration(int generationNumber);
        void LogGenerationInfo(GenerationResult<T, TScore> generationResult);
        void End();
    }
}