namespace GeneticSolver.Interfaces
{
    public interface IGenomeInfo<out T>
    {
        T Genome { get; }
        int Generation { get; }
    }
}