namespace GeneticSolver
{
    public class SolverParameters : ISolverParameters
    {
        public SolverParameters(int maxGenerationSize)
        {
            MaxGenerationSize = maxGenerationSize;
        }

        public int MaxGenerationSize { get; }
    }
}