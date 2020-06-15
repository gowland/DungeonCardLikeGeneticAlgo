using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GeneticSolver;
using System.Text;
using System.Threading.Tasks;
using DungeonCardsGeneticAlgo.Support;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.Expressions;
using GeneticSolver.Random;
using GeneticSolver.ReproductionStrategies;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo
{
    class Program
    {
        static void Main(string[] args)
        {
            var maxEliteSize = 1000;
            var cache = new FitnessCache<GameAgentMultipliers, double>(2000*maxEliteSize); // TODO: need to clear on repeated runs
            var evaluator = new GameAgentEvaluator(cache);

            var solverParameters = new SolverParameters(
                maxEliteSize,
                2*maxEliteSize,
                0.3);

            var expressionGenerator = new ExpressionGenerator(
                new BellWeightedRandom(0.5),
                new IExpression[]
                {
                    new BoundValueExpression(new DynamicValueSource<double>(() => solverParameters.InitialGenerationSize)),
                    new BoundValueExpression(new DynamicValueSource<double>(() => solverParameters.MaxEliteSize)),
                    new BoundValueExpression(new DynamicValueSource<double>(() => solverParameters.PropertyMutationProbability)),
                },
                new []
                {
                    new Operation((a,b) => a + b, "+"),
                    new Operation((a,b) => a - b, "-"),
                    new Operation((a,b) => a * b, "*"),
                });

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(expressionGenerator.GetRandomExpression());
            }

//            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
//                tasks.Add(Task.Run(() => LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator)));
                // LaunchEvolutionRun(solverParameters, evaluator);
            }

//            Task.WaitAll(tasks.ToArray());

//            TestBellCurve();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void LaunchEvolutionRun(SolverParameters solverParameters, GameAgentEvaluator evaluator)
        {
            var bellWeightedRandom = new CyclableBellWeightedRandom(10.0, 3.0, 1.0, 0.5, 0.1);
            var genomeDescriptions = new GameAgentMultipliersDescription(bellWeightedRandom);
            var defaultGenomeFactory = new GeneticSolver.Genome.DefaultGenomeFactory<GameAgentMultipliers>(genomeDescriptions);
            var mutationProbabilities = new Cyclable<double>(new[]{0.3, 0.9, 1.0});
            var mutator = new GenomeMutator<GameAgentMultipliers>(genomeDescriptions, mutationProbabilities, new UnWeightedRandom());
            var logger = new GameAgentSolverLogger();
            var solver = new Solver<GameAgentMultipliers, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new IEarlyStoppingCondition<GameAgentMultipliers, double>[]
                {
                    // new GeneticSolver.EarlyStoppingConditions.FitnessThresholdReachedEarlyStopCondition<GameAgentMultipliers, double>(fitness => fitness < 1e-6),
                    new GeneticSolver.EarlyStoppingConditions.ProgressStalledEarlyStoppingCondition<GameAgentMultipliers, double>(10, 0.5, 0.8),
                    new GeneticSolver.EarlyStoppingConditions.FitnessNotImprovingEarlyStoppingCondition<GameAgentMultipliers>(1, 100),
                },
                new IGenomeReproductionStrategy<GameAgentMultipliers>[]
                {
                    new CyclableReproductionStrategy(new IGenomeReproductionStrategy<GameAgentMultipliers>[]
                    {
                        new SexualGenomeReproductionStrategy<GameAgentMultipliers, double>(mutator, new StratifiedBreedingStrategy(),
                            defaultGenomeFactory, genomeDescriptions, evaluator, 40, 2),
                        new SexualGenomeReproductionStrategy<GameAgentMultipliers, double>(mutator, new GeneticSolver.PairingStrategies.RandomBreedingStrategy(),
                            defaultGenomeFactory, genomeDescriptions, evaluator, 40, 2),
                        new AsexualGenomeReproductionStrategy<GameAgentMultipliers>(new GenomeMutator<GameAgentMultipliers>(genomeDescriptions, 1.0, new UnWeightedRandom())),
                    }),
                    new AsexualGenomeReproductionStrategy<GameAgentMultipliers>(new GenomeMutator<GameAgentMultipliers>(genomeDescriptions, 0.4, new UnWeightedRandom())),
                });
            solver.NewGeneration += (s, e) => bellWeightedRandom.CycleStdDev();
            solver.NewGeneration += (s, e) => mutationProbabilities.Cycle();

            logger.Start();
            var best = solver.Evolve(1000);
            logger.LogGeneration(best);
            logger.End();
        }
    }

    public class CyclableReproductionStrategy : Cyclable<IGenomeReproductionStrategy<GameAgentMultipliers>>, IGenomeReproductionStrategy<GameAgentMultipliers>
    {
        public CyclableReproductionStrategy(IEnumerable<IGenomeReproductionStrategy<GameAgentMultipliers>> values) : base(values)
        {
        }

        public IEnumerable<GameAgentMultipliers> ProduceOffspring(IEnumerable<GameAgentMultipliers> parents)
        {
            return CurrentValue.ProduceOffspring(parents);
        }
    }
}
