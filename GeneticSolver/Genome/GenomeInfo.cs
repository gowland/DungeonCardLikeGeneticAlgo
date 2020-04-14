using GeneticSolver.Interfaces;

namespace GeneticSolver.Genome
{
    class GenomeInfo<T> : IGenomeInfo<T>
    {
        public GenomeInfo(T genome, int generation)
        {
            Genome = genome;
            Generation = generation;
        }

        public T Genome { get; }
        public int Generation { get; }
    }
}