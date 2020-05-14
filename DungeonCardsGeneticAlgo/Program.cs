using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;
using System.Text;
using System.Threading.Tasks;
using Game;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.GenomeProperty;
using GeneticSolver.Interfaces;
using GeneticSolver.Random;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo
{
    class Program
    {
        static void Main(string[] args)
        {
            var evaluator = new GameAgentEvaluator();
            var genomeDescriptions = new GameAgentMultipliersDescription();
            var defaultGenomeFactory = new GeneticSolver.Genome.DefaultGenomeFactory<GameAgentMultipliers>(genomeDescriptions);

            var solverParameters = new GeneticSolver.SolverParameters(
                100,
                200,
                0.3);

//            var tasks = new List<Task>();
            for (int i = 0; i < 1; i++)
            {
//                tasks.Add(Task.Run(() => LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator)));
                LaunchEvolutionRun(genomeDescriptions, solverParameters, defaultGenomeFactory, evaluator);
            }

//            Task.WaitAll(tasks.ToArray());

//            TestBellCurve();
            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        private static void LaunchEvolutionRun(GameAgentMultipliersDescription genomeDescriptions,
            SolverParameters solverParameters, GeneticSolver.Genome.DefaultGenomeFactory<GameAgentMultipliers> defaultGenomeFactory, GameAgentEvaluator evaluator)
        {
            var mutator = new BellWeightedGenomeMutator<GameAgentMultipliers>(genomeDescriptions, solverParameters.PropertyMutationProbability);
            var logger = new GameAgentSolverLogger();
            var solver = new Solver<GameAgentMultipliers, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new IEarlyStoppingCondition<GameAgentMultipliers, double>[]
                {
                    new GeneticSolver.EarlyStoppingConditions.FitnessThresholdReachedEarlyStopCondition<GameAgentMultipliers, double>(fitness => fitness < 1e-6),
                    new GeneticSolver.EarlyStoppingConditions.ProgressStalledEarlyStoppingCondition<GameAgentMultipliers, double>(10, 0.5, 0.8),
                    new GeneticSolver.EarlyStoppingConditions.FitnessNotImprovingEarlyStoppingCondition<GameAgentMultipliers>(1e-7, 10),
                },
                new IGenomeReproductionStrategy<GameAgentMultipliers>[]
                {
                    new GeneticSolver.ReproductionStrategies.SexualGenomeReproductionStrategy<GameAgentMultipliers, double>(mutator, new StratifiedBreedingStrategy(),
                        defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
                    new GeneticSolver.ReproductionStrategies.SexualGenomeReproductionStrategy<GameAgentMultipliers, double>(mutator, new GeneticSolver.PairingStrategies.RandomBreedingStrategy(),
                        defaultGenomeFactory, genomeDescriptions, evaluator, 100, 2),
                });
            solver.NewGeneration += (s, e) => mutator.CycleStdDev();

            logger.Start();
            var best = solver.Evolve(1000);
            logger.LogGeneration(best);
            logger.End();
        }
    }

    public class GameAgent
    {
        private readonly GameAgentMultipliers _multipliers;

        public GameAgent(GameAgentMultipliers multipliers)
        {
            _multipliers = multipliers;
        }

        public Game.DirectionResult GetDirectionFromAlgo(Game.Board board)
        {
            // Thread.Sleep(5000); // Allow humans to watch
            var moves = board.GetLegalMoves();

            var scoredMoves = moves.Select(pair => new { Direction = pair.Key, Score = GetScore(board, pair.Value) });

            return new Game.DirectionResult(scoredMoves.OrderByDescending(move => move.Score).First().Direction);
        }

        private double GetScore(Game.Board board, Game.Slot<Game.ICard<Game.CardType>> slot)
        {
            var card = slot.Card;
            SquareDesc squareDesc = board.Desc();
            switch (card.Type)
            {
                case Game.CardType.Monster:
                    return ScoreMonsterCard(board.Weapon, card.Value, board.HeroHealth, squareDesc);
                case Game.CardType.Weapon:
                    return ScoreWeaponCard(board.Weapon, card.Value, squareDesc);
                case Game.CardType.Gold:
                    return ScoreGoldCard(card.Value, squareDesc);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double ScoreGoldCard(int goldScore, SquareDesc squareDesc)
        {
            return _multipliers.GoldScoreMultiplier[(int)squareDesc] * goldScore;
        }

        private double ScoreWeaponCard(int heroWeapon, int weaponValue, SquareDesc squareDesc)
        {
            return heroWeapon > 0
                ? _multipliers.WeaponWhenPossessingWeaponScoreMultiplier[(int)squareDesc] * ScoreWeaponCardWhenPossessingWeapon(heroWeapon, weaponValue)
                : _multipliers.WeaponWhenPossessingNotWeaponScoreMultiplier[(int)squareDesc] * ScoreWeaponCardWhenNotPossessingWeapon(weaponValue);
        }

        private double ScoreWeaponCardWhenNotPossessingWeapon(int weaponValue)
        {
            return weaponValue;
        }

        private double ScoreWeaponCardWhenPossessingWeapon(int heroWeapon, int cardWeapon)
        {
            return (heroWeapon - cardWeapon);
        }

        private double ScoreMonsterCard(int heroWeapon, int monsterHealth, int heroHealth, SquareDesc squareDesc)
        {
            if (heroWeapon > 0)
            {
                return _multipliers.MonsterWhenPossessingWeaponScoreMultiplier[(int)squareDesc] * ScoreMonsterWhenPossessingWeapon(heroWeapon, monsterHealth);
            }
            else if (monsterHealth > heroHealth)
            {
                return ScoreMonsterWhenNotPossessingWeaponAndMonsterHealthIsGreater(heroHealth, monsterHealth);
            }
            else
            {
                return _multipliers.MonsterWhenNotPossessingWeaponScoreMultiplier[(int)squareDesc] * ScoreMonsterWhenNotPossessingWeaponAndHeroHealthIsGreater(heroHealth, monsterHealth);
            }
        }

        private double ScoreMonsterWhenNotPossessingWeaponAndMonsterHealthIsGreater(int heroHealth, int monsterHealth)
        {
            return int.MinValue;
        }

        private double ScoreMonsterWhenNotPossessingWeaponAndHeroHealthIsGreater(int heroHealth, int monsterHealth)
        {
            return (heroHealth - monsterHealth);
        }

        private double ScoreMonsterWhenPossessingWeapon(int heroWeapon, int monsterHealth)
        {
            return (monsterHealth - heroWeapon);
        }
    }

    public class GameAgentMultipliers : ICloneable
    {
        public GameAgentMultipliers()
        {
                GoldScoreMultiplier = new double[3];
                MonsterWhenPossessingWeaponScoreMultiplier = new double[3];
                MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3];
                WeaponWhenPossessingWeaponScoreMultiplier = new double[3];
                WeaponWhenPossessingNotWeaponScoreMultiplier = new double[3];
        }
        public double[] GoldScoreMultiplier { get; set; }
        public double[] MonsterWhenPossessingWeaponScoreMultiplier { get; set; }
        public double[] MonsterWhenNotPossessingWeaponScoreMultiplier { get; set; }
        public double[] WeaponWhenPossessingWeaponScoreMultiplier { get; set; }
        public double[] WeaponWhenPossessingNotWeaponScoreMultiplier { get; set; }
        public object Clone()
        {
            var clone = new GameAgentMultipliers();

            GoldScoreMultiplier.CopyTo(clone.GoldScoreMultiplier,0);
            MonsterWhenPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenPossessingWeaponScoreMultiplier, 0);
            MonsterWhenNotPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenNotPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingNotWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingNotWeaponScoreMultiplier, 0);

            return clone;
        }
    }

    public class GameAgentMultipliersDescription : IGenomeDescription<GameAgentMultipliers>
    {
        private readonly IRandom _random = new UnWeightedRandom();
        private readonly double _minChange = -5;
        private readonly double _maxChange = 5;
        public IEnumerable<IGenomeProperty<GameAgentMultipliers>> Properties => new[]
        {
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.GoldScoreMultiplier[0], (g, val) => g.GoldScoreMultiplier[0] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.GoldScoreMultiplier[1], (g, val) => g.GoldScoreMultiplier[1] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.GoldScoreMultiplier[2], (g, val) => g.GoldScoreMultiplier[2] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.MonsterWhenPossessingWeaponScoreMultiplier[0], (g, val) => g.MonsterWhenPossessingWeaponScoreMultiplier[0] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.MonsterWhenPossessingWeaponScoreMultiplier[1], (g, val) => g.MonsterWhenPossessingWeaponScoreMultiplier[1] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.MonsterWhenPossessingWeaponScoreMultiplier[2], (g, val) => g.MonsterWhenPossessingWeaponScoreMultiplier[2] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.MonsterWhenNotPossessingWeaponScoreMultiplier[0], (g, val) => g.MonsterWhenNotPossessingWeaponScoreMultiplier[0] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.MonsterWhenNotPossessingWeaponScoreMultiplier[1], (g, val) => g.MonsterWhenNotPossessingWeaponScoreMultiplier[1] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.MonsterWhenNotPossessingWeaponScoreMultiplier[2], (g, val) => g.MonsterWhenNotPossessingWeaponScoreMultiplier[2] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.WeaponWhenPossessingWeaponScoreMultiplier[0], (g, val) => g.WeaponWhenPossessingWeaponScoreMultiplier[0] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.WeaponWhenPossessingWeaponScoreMultiplier[1], (g, val) => g.WeaponWhenPossessingWeaponScoreMultiplier[1] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.WeaponWhenPossessingWeaponScoreMultiplier[2], (g, val) => g.WeaponWhenPossessingWeaponScoreMultiplier[2] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.WeaponWhenPossessingNotWeaponScoreMultiplier[0], (g, val) => g.WeaponWhenPossessingNotWeaponScoreMultiplier[0] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.WeaponWhenPossessingNotWeaponScoreMultiplier[1], (g, val) => g.WeaponWhenPossessingNotWeaponScoreMultiplier[1] = val, -100, 100, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentMultipliers>(g => g.WeaponWhenPossessingNotWeaponScoreMultiplier[2], (g, val) => g.WeaponWhenPossessingNotWeaponScoreMultiplier[2] = val, -100, 100, _minChange, _maxChange, _random),
        };
    }

    public class GameAgentEvaluator : IGenomeEvaluator<GameAgentMultipliers, double>
    {
        public IOrderedEnumerable<FitnessResult<GameAgentMultipliers, double>> GetFitnessResults(IEnumerable<IGenomeInfo<GameAgentMultipliers>> genomes)
        {
            return genomes.Select(genome => new FitnessResult<GameAgentMultipliers, double>(genome, GetFitness(genome.Genome)))
                .OrderByDescending(r => r.Fitness);
        }

        public IOrderedEnumerable<GameAgentMultipliers> GetFitnessResults(IEnumerable<GameAgentMultipliers> genomes)
        {
            return genomes.OrderByDescending(GetFitness);
        }

        private double GetFitness(GameAgentMultipliers genome)
        {
            return Enumerable.Range(1, 10).Select(_ => DoOneRun(genome)).Average();
        }

        private int DoOneRun(GameAgentMultipliers multipliers)
        {
            Board board = GameBuilder.GetRandomStartBoard();
            var gameAgent = new GameAgent(multipliers);
            var gameRunner = new GameRunner(gameAgent.GetDirectionFromAlgo, _ => {});
            return gameRunner.RunGame(board);
        }
    }

    public class GameAgentSolverLogger : ISolverLogger<GameAgentMultipliers, double>
    {
        private readonly Guid _runId;

        public GameAgentSolverLogger()
        {
            _runId = Guid.NewGuid();
        }
        public void Start()
        {
            Console.WriteLine($"Starting {_runId}");
        }

        public void LogStartGeneration(int generationNumber)
        {
        }

        public void LogGenerationInfo(IGenerationResult<GameAgentMultipliers, double> generationResult)
        {
            Console.WriteLine($"{_runId},{generationResult.GenerationNumber},{generationResult.FittestGenome.Fitness}");
        }

        public void LogGeneration(IGenerationResult<GameAgentMultipliers, double> generation)
        {
            var best = generation.FittestGenome.GenomeInfo.Genome;
            Console.WriteLine($"Gold multipliers              {string.Join(", ", best.GoldScoreMultiplier.Select(d => $"{d:0.0000}"))}");
            Console.WriteLine($"Monster w/ weapon multipliers {string.Join(", ", best.MonsterWhenPossessingWeaponScoreMultiplier.Select(d => $"{d:0.0000}"))}");
            Console.WriteLine($"Monster no weapon multipliers {string.Join(", ", best.MonsterWhenNotPossessingWeaponScoreMultiplier.Select(d => $"{d:0.0000}"))}");
            Console.WriteLine($"Weapon w/ weapon multipliers  {string.Join(", ", best.WeaponWhenPossessingWeaponScoreMultiplier.Select(d => $"{d:0.0000}"))}");
            Console.WriteLine($"Weapon no weapon multipliers  {string.Join(", ", best.WeaponWhenPossessingNotWeaponScoreMultiplier.Select(d => $"{d:0.0000}"))}");
        }

        public void End()
        {
            Console.WriteLine($"Fin {_runId}");
        }
    }
}
