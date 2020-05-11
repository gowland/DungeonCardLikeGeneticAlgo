using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgo.Coefficients;
using GeneticSolver;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.EarlyStoppingConditions;
using GeneticSolver.Genome;
using GeneticSolver.Random;
using GeneticSolver.ReproductionStrategies;
using GeneticSolver.RequiredInterfaces;

namespace GeneticAlgo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
//            var solver = new Solver<Values.MyThing, int>(new DefaultGenomeFactory<MyThing>(), new MyThingGenomeEvaluator(), new MyThingGenomeDescription(), new MyThingSolverLogger(), new SolverParameters(1000));

            var coefficientsToMatch = new Coefficients.Coefficients
            {
                FifthLevel = 0,
                FourthLevel = 3.9,
                ThirdLevel = -34.5,
                SecondLevel = -9.8,
                FirstLevel = 11.9
            };
            var pointsToMatch = Enumerable.Range(-1000,1000).Select(x => new Point(x, coefficientsToMatch.Calc(x)));
            var evaluator = new CoefficientsGenomeEvaluator(pointsToMatch);
            var genomeDescriptions = new CoefficientsGenomeDescriptions();
            var defaultGenomeFactory = new DefaultGenomeFactory<Coefficients.Coefficients>(genomeDescriptions);

            var solverParameters = new SolverParameters(
                1000,
                2000,
                0.3);

//            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
//                tasks.Add(Task.Run(() => LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator)));
                LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator);
            }

//            Task.WaitAll(tasks.ToArray());

//            TestBellCurve();
            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static void TestBellCurve()
        {
            var r = new BellWeightedRandom(0.1);
            var d = new Dictionary<double, int>()
            {
                [0.1] = 0,
                [0.2] = 0,
                [0.3] = 0,
                [0.4] = 0,
                [0.5] = 0,
                [0.6] = 0,
                [0.7] = 0,
                [0.8] = 0,
                [0.9] = 0,
                [1.0] = 0,
            };

            for (int i = 0; i < 100; i++)
            {
                var nextDouble = r.NextDouble();
//                Console.WriteLine(nextDouble);

                double bucket = d.Keys.First(key => nextDouble < key);
                d[bucket]++;
            }

            foreach (var pair in d)
            {
                Console.WriteLine($"{pair.Key:0.00} -> {pair.Value}");
            }
        }

        private static void LaunchEvolutionRun(CoefficientsGenomeDescriptions genomeDescriptions,
            SolverParameters solverParameters, DefaultGenomeFactory<Coefficients.Coefficients> defaultGenomeFactory, CoefficientsGenomeEvaluator evaluator)
        {
            var mutator = new BellWeightedGenomeMutator<Coefficients.Coefficients>(genomeDescriptions, solverParameters.PropertyMutationProbability);
            var logger = new CoefficientsSolverLogger();
            var solver = new Solver<Coefficients.Coefficients, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new IEarlyStoppingCondition<Coefficients.Coefficients, double>[]
                {
                    new FitnessThresholdReachedEarlyStopCondition<Coefficients.Coefficients, double>(fitness => fitness < 1e-6),
                    new ProgressStalledEarlyStoppingCondition<Coefficients.Coefficients, double>(10, 0.5, 0.8),
                    new FitnessNotImprovingEarlyStoppingCondition<Coefficients.Coefficients>(1e-7, 10),
                },
                new IGenomeReproductionStrategy<Coefficients.Coefficients>[]
                {
//                    new SexualGenomeReproductionStrategy<Coefficients, double>(mutator, new HaremBreedingStrategy(),
//                        defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
                    new SexualGenomeReproductionStrategy<Coefficients.Coefficients, double>(mutator, new StratifiedBreedingStrategy(), 
                        defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
//                    new SexualGenomeReproductionStrategy<Coefficients, double>(mutator, new RandomBreedingStrategy(), 
//                        defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
//                    new AsexualGenomeReproductionStrategy<Coefficients>(mutator), 
                });
            solver.NewGeneration += (s, e) => mutator.CycleStdDev();

            logger.Start();
            var best = solver.Evolve(1000);
            logger.LogGeneration(best);
            logger.End();
        }
    }
}