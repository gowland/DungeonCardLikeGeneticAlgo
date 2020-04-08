namespace GeneticSolver
{
    public interface IGenomeInfo<out T>
    {
        T Genome { get; }
        int Generation { get; }
    }
}