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
            IGameAgent agent = new ReinforcementLearningGameAgent(initialDecisionScorer);

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

            while (Console.ReadKey().Key != ConsoleKey.X)
            {
                GameBuilder.RandomizeBoardToStart(board);
                var score = gameRunner.RunGame(board);
                trainer.Train();
                trainer.Dump();
                Console.WriteLine($"Game score {score}");
            }
        }
    }

    public class ReinforcementLearningGameAgent : GameAgentBase
    {
        private readonly IDecisionScorer _scorer;

        public ReinforcementLearningGameAgent(IDecisionScorer scorer)
        {
            _scorer = scorer;
        }
        protected override double GetScore(Board board, ISlot<ICard<CardType>> slot)
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
    }

    public class GameState : IEquatable<GameState>
    {
        public int HeroHealth { get; set; }
        public int HeroWeapon { get; set; }

        public int TotalBoardMonster { get; set; }
        public int TotalBoardWeapon { get; set; }
        public int TotalBoardGold { get; set; }

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
                HeroHealth = board.HeroHealth,
                HeroWeapon = board.Weapon,

                TotalBoardMonster = board.GetSlots().Where(s => s.Card.Type == CardType.Monster).Sum(s => s.Card.Value),
                TotalBoardWeapon = board.GetSlots().Where(s => s.Card.Type == CardType.Weapon).Sum(s => s.Card.Value),
                TotalBoardGold = board.GetSlots().Where(s => s.Card.Type == CardType.Gold).Sum(s => s.Card.Value),

                HeroNeighborMonster = board.GetCurrentLegalMoves().Where(s => s.Value.Card.Type == CardType.Monster).Sum(s => s.Value.Card.Value),
                HeroNeighborWeapon = board.GetCurrentLegalMoves().Where(s => s.Value.Card.Type == CardType.Weapon).Sum(s => s.Value.Card.Value),
                HeroNeighborGold = board.GetCurrentLegalMoves().Where(s => s.Value.Card.Type == CardType.Gold).Sum(s => s.Value.Card.Value),
            };
        }

        public override string ToString()
        {
            return $"HERO [H:{HeroHealth}, W:{HeroWeapon}], BOARD [M:{TotalBoardMonster}, W:{TotalBoardWeapon}, G:{TotalBoardGold}], NEIGHBOR [M:{HeroNeighborMonster}, W:{HeroNeighborWeapon}, G:{HeroNeighborGold}]";
        }

        public bool Equals(GameState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HeroHealth == other.HeroHealth && HeroWeapon == other.HeroWeapon && TotalBoardMonster == other.TotalBoardMonster && TotalBoardWeapon == other.TotalBoardWeapon && TotalBoardGold == other.TotalBoardGold && HeroNeighborMonster == other.HeroNeighborMonster && HeroNeighborWeapon == other.HeroNeighborWeapon && HeroNeighborGold == other.HeroNeighborGold;
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
                var hashCode = HeroHealth;
                hashCode = (hashCode * 397) ^ HeroWeapon;
                hashCode = (hashCode * 397) ^ TotalBoardMonster;
                hashCode = (hashCode * 397) ^ TotalBoardWeapon;
                hashCode = (hashCode * 397) ^ TotalBoardGold;
                hashCode = (hashCode * 397) ^ HeroNeighborMonster;
                hashCode = (hashCode * 397) ^ HeroNeighborWeapon;
                hashCode = (hashCode * 397) ^ HeroNeighborGold;
                return hashCode;
            }
        }
    }

    public class MoveScore : IComparable<MoveScore>
    {
        public double ChangeInHealth { get; set; }
        public double LossOfWeapon { get; set; }
        public double ChangeInWeapon { get; set; }

        public double ChangeInBoardMonster { get; set; }
        public double ChangeInBoardWeapon { get; set; }
        public double ChangeInBoardGold { get; set; }

        public double ChangeInHeroNeighborMonster { get; set; }
        public double ChangeInHeroNeighborWeapon { get; set; }
        public double ChangeInHeroNeighborGold { get; set; }

        public double Score => -2 * ChangeInHealth + ChangeInBoardMonster + ChangeInBoardWeapon + ChangeInHeroNeighborMonster + ChangeInHeroNeighborWeapon; //TODO: Update

        public static MoveScore FromChangeInState(Decision decision, GameState newState)
        {
            return new MoveScore()
            {
                LossOfWeapon = 0, // TODO: How do I calculate this?
                ChangeInHealth = decision.State.HeroHealth - newState.HeroHealth,
                ChangeInWeapon = decision.State.HeroWeapon - newState.HeroWeapon, // TODO: This isn't a bad thing if a monster got killed or injured
                ChangeInBoardMonster = decision.State.TotalBoardMonster - newState.TotalBoardMonster,
                ChangeInBoardWeapon = decision.State.TotalBoardWeapon - newState.TotalBoardWeapon,
                ChangeInBoardGold = decision.State.TotalBoardGold - newState.TotalBoardGold,
                ChangeInHeroNeighborMonster = decision.State.HeroNeighborMonster - newState.HeroNeighborMonster,
                ChangeInHeroNeighborWeapon = decision.State.HeroNeighborWeapon - newState.HeroNeighborWeapon,
                ChangeInHeroNeighborGold = decision.State.HeroNeighborGold - newState.HeroNeighborGold,
            };
        }

        public override string ToString()
        {
            return $"CHANGES Health: {ChangeInHealth} + BOARD[M:{ChangeInBoardMonster} + W:{ChangeInBoardWeapon}] + NEIGHBOR[M:{ChangeInHeroNeighborMonster} + W:{ChangeInHeroNeighborWeapon}] = score:{Score}"; //TODO: Update
        }

        public int CompareTo(MoveScore other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Score.CompareTo(other.Score);
        }
    }

    public class Decision
    {
        public GameState State { get; set; }
        public SlotState SlotState { get; set; }

        public override string ToString()
        {
            return $"({State}),({SlotState})";
        }
    }

    public class SlotState
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
                currentScore.ChangeInWeapon = GetUpdatedValue(currentScore.ChangeInWeapon, newScore.ChangeInWeapon, 0.1);
                currentScore.ChangeInHealth = GetUpdatedValue(currentScore.ChangeInHealth, newScore.ChangeInHealth, 0.1);
                currentScore.LossOfWeapon = GetUpdatedValue(currentScore.LossOfWeapon, newScore.LossOfWeapon, 0.1);

                currentScore.ChangeInBoardMonster = GetUpdatedValue(currentScore.ChangeInBoardMonster, newScore.ChangeInBoardMonster, 0.1);
                currentScore.ChangeInBoardWeapon = GetUpdatedValue(currentScore.ChangeInBoardWeapon, newScore.ChangeInBoardWeapon, 0.1);
                currentScore.ChangeInBoardGold = GetUpdatedValue(currentScore.ChangeInBoardGold, newScore.ChangeInBoardGold, 0.1);

                currentScore.ChangeInHeroNeighborMonster = GetUpdatedValue(currentScore.ChangeInHeroNeighborMonster, newScore.ChangeInHeroNeighborMonster, 0.1);
                currentScore.ChangeInHeroNeighborWeapon = GetUpdatedValue(currentScore.ChangeInHeroNeighborWeapon, newScore.ChangeInHeroNeighborWeapon, 0.1);
                currentScore.ChangeInHeroNeighborGold = GetUpdatedValue(currentScore.ChangeInHeroNeighborGold, newScore.ChangeInHeroNeighborGold, 0.1);
            }
            else
            {
                _choices[decision] = newScore;
            }
        }

        public void Dump()
        {
            foreach (var pair in _choices)
            {
                Console.WriteLine($"[{pair.Key}] = {pair.Value}");
            }
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
            _accumulatedScores.Add(new Tuple<Decision, MoveScore>(_latestDecision, MoveScore.FromChangeInState(_latestDecision, newState) ));
            _latestDecision = null;
        }

        public void Train()
        {
            foreach (var score in _accumulatedScores)
            {
                _initialScores.UpdateScores(score.Item1, score.Item2);
            }
        }

        public void Dump()
        {
            _initialScores.Dump();
        }
    }
}