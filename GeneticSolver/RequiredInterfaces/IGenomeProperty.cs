namespace GeneticSolver.RequiredInterfaces
{
    public interface IGenomeProperty<in T>
    {
        void Mutate(T genome);
        void Mutate(T genome, IRandom random);
        void SetRandom(T genome);
        void Merge(T parent1, T parent2, T child);
    }
}