namespace GeneticSolver.RequiredInterfaces
{
    public interface IRandom
    {
        double NextDouble();
        double NextDouble(double minX, double maxX);
    }
}