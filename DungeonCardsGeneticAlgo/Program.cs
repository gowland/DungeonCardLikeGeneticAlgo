using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;
using System.Text;
using System.Threading.Tasks;
using GeneticSolver.BreedingStrategies;
using GeneticSolver.Interfaces;
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

        private static void LaunchEvolutionRun(GameAgentMultipliersDescription genomeDescriptions,
            GeneticSolver.SolverParameters solverParameters, GeneticSolver.Genome.DefaultGenomeFactory<GameAgentMultipliers> defaultGenomeFactory, GameAgentEvaluator evaluator)
        {
            var mutator = new BellWeightedGenomeMutator<GameAgentMultipliers>(genomeDescriptions, solverParameters.PropertyMutationProbability);
            var logger = new GameAgentSolverLogger();
            var solver = new GeneticSolver.Solver<GameAgentMultipliers, double>(
                defaultGenomeFactory,
                evaluator,
                logger,
                solverParameters,
                new GeneticSolver.RequiredInterfaces.IEarlyStoppingCondition<GameAgentMultipliers, double>[]
                {
                    new GeneticSolver.EarlyStoppingConditions.FitnessThresholdReachedEarlyStopCondition<GameAgentMultipliers, double>(fitness => fitness < 1e-6),
                    new GeneticSolver.EarlyStoppingConditions.ProgressStalledEarlyStoppingCondition<GameAgentMultipliers, double>(10, 0.5, 0.8),
                    new GeneticSolver.EarlyStoppingConditions.FitnessNotImprovingEarlyStoppingCondition<GameAgentMultipliers>(1e-7, 10),
                },
                new GeneticSolver.RequiredInterfaces.IGenomeReproductionStrategy<GameAgentMultipliers>[]
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
            switch (card.Type)
            {
                case Game.CardType.Monster:
                    return ScoreMonsterCard(board.Weapon, card.Value, board.HeroHealth);
                case Game.CardType.Weapon:
                    return ScoreWeaponCard(board.Weapon, card.Value);
                case Game.CardType.Gold:
                    return ScoreGoldCard(card.Value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double ScoreGoldCard(int goldScore)
        {
            return _multipliers.GoldScoreMultiplier * goldScore;
        }

        private double ScoreWeaponCard(int heroWeapon, int weaponValue)
        {
            return heroWeapon > 0
                ? ScoreWeaponCardWhenPossessingWeapon(heroWeapon, weaponValue)
                : ScoreWeaponCardWhenNotPossessingWeapon(weaponValue);
        }

        private double ScoreWeaponCardWhenNotPossessingWeapon(int weaponValue)
        {
            return _multipliers.WeaponWhenPossessingNotWeaponScoreMultiplier * weaponValue;
        }

        private double ScoreWeaponCardWhenPossessingWeapon(int heroWeapon, int cardWeapon)
        {
            return _multipliers.WeaponWhenPossessingWeaponScoreMultiplier * (heroWeapon - cardWeapon);
        }

        private double ScoreMonsterCard(int heroWeapon, int monsterHealth, int heroHealth)
        {
            if (heroWeapon > 0)
            {
                return ScoreMonsterWhenPossessingWeapon(heroWeapon, monsterHealth);
            }
            else if (monsterHealth > heroHealth)
            {
                return ScoreMonsterWhenNotPossessingWeaponAndHeroHealthIsGreater();
            }
            else
            {
                return ScoreMonsterWhenNotPossessingWeaponAndMonsterHealthIsGreater(heroHealth, monsterHealth);
            }
        }

        private double ScoreMonsterWhenNotPossessingWeaponAndMonsterHealthIsGreater(int heroHealth, int monsterHealth)
        {
            return _multipliers.MonsterWhenNotPossessingWeaponScoreMultiplier * (heroHealth - monsterHealth);
        }

        private double ScoreMonsterWhenNotPossessingWeaponAndHeroHealthIsGreater()
        {
            return int.MinValue;
        }

        private double ScoreMonsterWhenPossessingWeapon(int heroWeapon, int monsterHealth)
        {
            return _multipliers.MonsterWhenPossessingWeaponScoreMultiplier * (monsterHealth - heroWeapon);
        }
    }

    public class GameAgentMultipliers : ICloneable
    {
        public double GoldScoreMultiplier { get; set; }
        public double MonsterWhenPossessingWeaponScoreMultiplier { get; set; }
        public double MonsterWhenNotPossessingWeaponScoreMultiplier { get; set; }
        public double WeaponWhenPossessingWeaponScoreMultiplier { get; set; }
        public double WeaponWhenPossessingNotWeaponScoreMultiplier { get; set; }
        public object Clone()
        {
            var clone = new GameAgentMultipliers()
            {
                GoldScoreMultiplier = GoldScoreMultiplier,
                MonsterWhenPossessingWeaponScoreMultiplier = MonsterWhenPossessingWeaponScoreMultiplier,
                MonsterWhenNotPossessingWeaponScoreMultiplier = MonsterWhenNotPossessingWeaponScoreMultiplier,
                WeaponWhenPossessingWeaponScoreMultiplier = WeaponWhenPossessingWeaponScoreMultiplier,
                WeaponWhenPossessingNotWeaponScoreMultiplier = WeaponWhenPossessingNotWeaponScoreMultiplier,
            };

            return clone;
        }
    }

    public class GameAgentMultipliersDescription : GeneticSolver.IGenomeDescription<GameAgentMultipliers>
    {
        public IEnumerable<IGenomeProperty<GameAgentMultipliers>> Properties { get; }
    }

    public class GameAgentEvaluator : GeneticSolver.RequiredInterfaces.IGenomeEvaluator<GameAgentMultipliers, double>
    {
        public IOrderedEnumerable<FitnessResult<GameAgentMultipliers, double>> GetFitnessResults(IEnumerable<IGenomeInfo<GameAgentMultipliers>> genomes)
        {
            throw new NotImplementedException();
        }

        public IOrderedEnumerable<GameAgentMultipliers> GetFitnessResults(IEnumerable<GameAgentMultipliers> genomes)
        {
            throw new NotImplementedException();
        }
    }

    public class GameAgentSolverLogger : GeneticSolver.RequiredInterfaces.ISolverLogger<GameAgentMultipliers, double>
    {
        public void Start()
        {
            throw new NotImplementedException();
        }

        public void LogStartGeneration(int generationNumber)
        {
            throw new NotImplementedException();
        }

        public void LogGenerationInfo(IGenerationResult<GameAgentMultipliers, double> generationResult)
        {
            throw new NotImplementedException();
        }

        public void LogGeneration(IGenerationResult<GameAgentMultipliers, double> generation)
        {
            throw new NotImplementedException();
        }

        public void End()
        {
            throw new NotImplementedException();
        }
    }
}
