namespace GeneticSolver.RequiredInterfaces
{
    public interface IMutator<in T>
    {
        void Mutate(T genome);
    }
}