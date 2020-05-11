using System;
using System.IO;
using System.Linq;
using GeneticSolver;
using GeneticSolver.RequiredInterfaces;

namespace GeneticAlgo.Coefficients
{
    public class CoefficientsSolverLogger : ISolverLogger<Coefficients, double>
    {
        private StreamWriter _logFile;
        private readonly Guid _loggerId = Guid.NewGuid();

        public void Start()
        {
            _logFile = new StreamWriter($"log_{_loggerId}.csv");
            _logFile.WriteLine("Run ID,Generation,Average Age,Top 10 Average Age,Best Age,Average Error,Top 10 Error, Best Error");
            Console.WriteLine($"{_loggerId} Start: {DateTime.Now:g}");
        }

        public void LogStartGeneration(int generationNumber)
        {
//            Console.WriteLine($"---------- {generationNumber} ---------- ");
        }

        public void LogGenerationInfo(IGenerationResult<Coefficients, double> generationResult)
        {
//            Console.WriteLine($" Average generation: {generationResult.AverageGenomeGeneration:0.00}");
//            Console.WriteLine($" Average fitness: {generationResult.OrderedGenomes.Average(r => r.Fitness):e2}");
            LogGenome(generationResult.FittestGenome);

            FitnessResult<Coefficients, double> topGenome = generationResult.FittestGenome;
            double averageAgeTop10Genomes = generationResult.OrderedGenomes.Take(10).Average(r => r.GenomeInfo.Generation);
            var averageScoreAllGenomes = generationResult.OrderedGenomes.Average(r => r.Fitness);
            var averageScoreTop10Genomes = generationResult.OrderedGenomes.Take(10).Average(r => r.Fitness);
            _logFile.WriteLine($"{_loggerId},{generationResult.GenerationNumber},{generationResult.AverageGenomeGeneration:0.00},{averageAgeTop10Genomes:0.00},{topGenome.GenomeInfo.Generation:0},{averageScoreAllGenomes:e2},{averageScoreTop10Genomes:e2},{topGenome.Fitness:e2}");
            _logFile.Flush();
        }

        private void LogGenome(FitnessResult<Coefficients, double> result)
        {
            var coefficients = result.GenomeInfo.Genome;
            Console.WriteLine($" {result.GenomeInfo.Generation}, {result.Fitness:e2} - ({coefficients.FifthLevel:0.##}) * x ^ 4 + ({coefficients.FourthLevel:0.##}) * x ^ 3 + ({coefficients.ThirdLevel:0.##}) * x ^ 2 + ({coefficients.SecondLevel:0.##}) * x + ({coefficients.FirstLevel:0.##})");
        }

        public void End()
        {
            Console.WriteLine($"{_loggerId} Fin: {DateTime.Now:g}");
            _logFile.Flush();
            _logFile.Close();
            _logFile = null;
        }

        public void LogGeneration(IGenerationResult<Coefficients, double> generationResult)
        {
            using (var generationFile = new StreamWriter($"generation_{generationResult.GenerationNumber}_{_loggerId}.csv"))
            {
                foreach (var fitnessResult in generationResult.OrderedGenomes)
                {
                    Coefficients genome = fitnessResult.GenomeInfo.Genome;
                    generationFile.WriteLine($"{fitnessResult.Fitness:e2},{fitnessResult.GenomeInfo.Generation},{genome.FifthLevel:0.0000},{genome.FourthLevel:0.0000},{genome.ThirdLevel:0.0000},{genome.SecondLevel:0.0000},{genome.FirstLevel:0.0000}");
                }
            }
        }
    }
}