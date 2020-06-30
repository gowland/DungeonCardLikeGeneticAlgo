using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Player;

namespace ReinforcementLearning
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var initialDecisionScorer = new DecisionScores();
            IGameAgent agent = new GameStateGameAgent(new ProbabilityBasedMoveSelector(), initialDecisionScorer);

            var trainer = new Trainer(initialDecisionScorer);

            Board board = GameBuilder.GetRandomStartBoard();
            GameRunner gameRunner = new GameRunner(agent, _ => {});

            gameRunner.DirectionChosen += (sender, direction) => trainer.SetLatestDecision(new Decision()
            {
                SlotState = SlotState.FromSlot(board.GetSlotNextToHero(direction)),
                State = GameState.FromBoard(board),
            });

            gameRunner.StateChanged += (sender, eventArgs) =>
                trainer.SetStateAfterLatestDecision(GameState.FromBoard(board));

            // while (Console.ReadKey().Key != ConsoleKey.X)
            for (int trainingBatch = 0; trainingBatch < 10000; trainingBatch++)
            {
                for(int i=0; i<10000; i++)
                {
                    GameBuilder.RandomizeBoardToStart(board);
                    var score = gameRunner.RunGame(board);
                    trainer.Train(score);

                    // Console.WriteLine($"Game score {score}");
                }

                var currentEvaluationScore = Evaluate(initialDecisionScorer);
                Console.WriteLine($"{trainingBatch:0000}:{currentEvaluationScore}");
            }

            Console.ReadKey();

            initialDecisionScorer.DumpChoices();
        }

        private static double Evaluate(DecisionScores scores)
        {
            Board board = GameBuilder.GetRandomStartBoard();

            var fitnesses = Enumerable.Range(1, 100)
                .Select(_ => (double)DoOneRun(board, scores))
                .ToArray();

            var fitness = fitnesses.Average() + fitnesses.StdDev();
            return fitness;
        }

        private static int DoOneRun(Board board, DecisionScores scores)
        {
            GameBuilder.RandomizeBoardToStart(board);
            IGameAgent agent = new GameStateGameAgent(new MaximalMoveSelector(), scores);
            var gameRunner = new GameRunner(agent, _ => {});
            return gameRunner.RunGame(board);
        }
    }

    public class GameStateGameAgent : IGameAgent
    {
        private readonly IMoveSelector _itemSelector;
        private readonly IDecisionScorer _scorer;

        public GameStateGameAgent(IMoveSelector itemSelector, IDecisionScorer scorer)
        {
            _itemSelector = itemSelector;
            _scorer = scorer;
        }
        protected double GetScore(Board board, ISlot<ICard<CardType>> slot)
        {
            var state = GameState.FromBoard(board);
            var slotState = SlotState.FromSlot(slot);
            var decision = new Decision()
            {
                State = state,
                SlotState = slotState,
            };

            return _scorer.GetScoresForState(decision);
        }

        public DirectionResult GetDirectionFromAlgo(Board board)
        {
            return _itemSelector.GetDirectionFromAlgo(board, this.GetScore);
        }
    }

    public class GameState : IEquatable<GameState>
    {
        // public int HeroHealth { get; set; }
        public int HeroWeapon { get; set; }

        /*
        public int TotalBoardMonster { get; set; }
        public int TotalBoardWeapon { get; set; }
        public int TotalBoardGold { get; set; }
        */

        public int HeroNeighborMonster { get; set; }
        public int HeroNeighborWeapon { get; set; }
        public int HeroNeighborGold { get; set; }

        // position : corner / side / center
        // neighbors : L/R/U/D -> monster / weapon / code /
        // look ahead: count of different types of cards of neighbors?

        public static GameState FromBoard(Board board)
        {
            return new GameState()
            {
                // HeroHealth = board.HeroHealth,
                HeroWeapon = board.Weapon,

                /*
                TotalBoardMonster = board.GetSlots().Where(s => s.Card.Type == CardType.Monster).Sum(s => s.Card.Value),
                TotalBoardWeapon = board.GetSlots().Where(s => s.Card.Type == CardType.Weapon).Sum(s => s.Card.Value),
                TotalBoardGold = board.GetSlots().Where(s => s.Card.Type == CardType.Gold).Sum(s => s.Card.Value),
                */

                // HeroNeighborMonster = board.GetCurrentLegalMoves().Where(s => s.Value.Card.Type == CardType.Monster).Sum(s => s.Value.Card.Value),
                // HeroNeighborWeapon = board.GetCurrentLegalMoves().Where(s => s.Value.Card.Type == CardType.Weapon).Sum(s => s.Value.Card.Value),
                // HeroNeighborGold = board.GetCurrentLegalMoves().Where(s => s.Value.Card.Type == CardType.Gold).Sum(s => s.Value.Card.Value),
                HeroNeighborMonster = board.GetCurrentLegalMoves().Count(s => s.Value.Card.Type == CardType.Monster),
                HeroNeighborWeapon = board.GetCurrentLegalMoves().Count(s => s.Value.Card.Type == CardType.Weapon),
                HeroNeighborGold = board.GetCurrentLegalMoves().Count(s => s.Value.Card.Type == CardType.Gold),
            };
        }

        public override string ToString()
        {
            // return $"HERO [H:{HeroHealth}, W:{HeroWeapon}], BOARD [M:{TotalBoardMonster}, W:{TotalBoardWeapon}, G:{TotalBoardGold}], NEIGHBOR [M:{HeroNeighborMonster}, W:{HeroNeighborWeapon}, G:{HeroNeighborGold}]";
            // return $"HERO [H:{HeroHealth}, W:{HeroWeapon}], NEIGHBOR [M:{HeroNeighborMonster}, W:{HeroNeighborWeapon}, G:{HeroNeighborGold}]";
            return $"HERO [W:{HeroWeapon}], NEIGHBOR [M:{HeroNeighborMonster}, W:{HeroNeighborWeapon}, G:{HeroNeighborGold}]";
        }

        public bool Equals(GameState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HeroWeapon == other.HeroWeapon && HeroNeighborMonster == other.HeroNeighborMonster && HeroNeighborWeapon == other.HeroNeighborWeapon && HeroNeighborGold == other.HeroNeighborGold;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HeroWeapon;
                hashCode = (hashCode * 397) ^ HeroNeighborMonster;
                hashCode = (hashCode * 397) ^ HeroNeighborWeapon;
                hashCode = (hashCode * 397) ^ HeroNeighborGold;
                return hashCode;
            }
        }
    }

    public class MoveScore : IComparable<MoveScore>
    {
        public double Gold { get; set; }

        public double Score => Gold;

        public int CompareTo(MoveScore other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Gold.CompareTo(other.Gold);
        }

        public override string ToString()
        {
            return $"{Score}";
        }
    }

    public class Decision : IEquatable<Decision>
    {
        public GameState State { get; set; }
        public SlotState SlotState { get; set; }

        public override string ToString()
        {
            return $"({State}),({SlotState})";
        }

        public bool Equals(Decision other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(State, other.State) && Equals(SlotState, other.SlotState);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Decision) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((State != null ? State.GetHashCode() : 0) * 397) ^ (SlotState != null ? SlotState.GetHashCode() : 0);
            }
        }
    }

    public class SlotState : IEquatable<SlotState>
    {
        public int CardGold { get; set; }
        public int CardWeapon { get; set; }
        public int CardMonster { get; set; }

        public static SlotState FromSlot(ISlot<ICard<CardType>> slot)
        {
            var state = new SlotState();

            switch (slot.Card.Type)
            {
                case CardType.Monster:
                    state.CardMonster = slot.Card.Value;
                    break;
                case CardType.Weapon:
                    state.CardWeapon = slot.Card.Value;
                    break;
                case CardType.Gold:
                    state.CardGold = slot.Card.Value;
                    break;
            }

            return state;
        }

        public override string ToString()
        {
            return $"cardGold{CardGold}, cardMonster{CardMonster}, cardWeapon{CardWeapon}";
        }

        public bool Equals(SlotState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CardGold == other.CardGold && CardWeapon == other.CardWeapon && CardMonster == other.CardMonster;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SlotState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CardGold;
                hashCode = (hashCode * 397) ^ CardWeapon;
                hashCode = (hashCode * 397) ^ CardMonster;
                return hashCode;
            }
        }
    }

    public interface IDecisionScorer
    {
        double GetScoresForState(Decision decision);
    }

    public interface IDecisionUpdater
    {
        void UpdateScores(Decision decision, MoveScore newScore);
    }

    public class DecisionScores : IDecisionScorer, IDecisionUpdater
    {
        private readonly IDictionary<Decision, MoveScore> _choices = new Dictionary<Decision, MoveScore>();

        public double GetScoresForState(Decision decision)
        {
            if (_choices.TryGetValue(decision, out MoveScore score))
            {
                return score.Score;
            }

            return 0.1;
        }

        public void UpdateScores(Decision decision, MoveScore newScore)
        {
            if (_choices.TryGetValue(decision, out MoveScore currentScore))
            {
                currentScore.Gold = GetUpdatedValue(currentScore.Gold, newScore.Gold, 0.01);
            }
            else
            {
                _choices[decision] = newScore;
            }
        }

        public void Dump()
        {
            Console.WriteLine($"Total states: {_choices.Count}");
        }

        public void DumpChoices()
        {
            foreach (var pair in _choices)
            {
                Console.WriteLine($"[{pair.Key}] = {pair.Value}");
            }

            Console.WriteLine($"Total states: {_choices.Count}");
        }

        private double GetUpdatedValue(double startValue, double newValue, double learningRate)
        {
            var diff = newValue - startValue;
            return startValue + learningRate * diff;
        }
    }

    public class Trainer
    {
        private readonly DecisionScores _initialScores;

        private IList<Tuple<Decision, MoveScore>> _accumulatedScores = new List<Tuple<Decision, MoveScore>>();

        private Decision _latestDecision;

        public Trainer(DecisionScores initialScores)
        {
            _initialScores = initialScores;
        }

        public void SetLatestDecision(Decision decision)
        {
            _latestDecision = decision;
        }

        public void SetStateAfterLatestDecision(GameState newState)
        {
            _accumulatedScores.Add(new Tuple<Decision, MoveScore>(_latestDecision, new MoveScore{Gold = 0} ));
            _latestDecision = null;
        }

        public void Train(int gold)
        {
            foreach (var score in _accumulatedScores)
            {
                score.Item2.Gold += gold;
                _initialScores.UpdateScores(score.Item1, score.Item2);
            }

            _accumulatedScores.Clear();
        }

        public void Dump()
        {
            _initialScores.Dump();
        }
    }
}