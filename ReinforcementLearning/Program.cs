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

            int score = gameRunner.RunGame(board);

            trainer.Train();

            Console.WriteLine($"Game score {score}");
            Console.ReadLine();
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

            var score = _scorer.GetScoresForState(decision);

            return score.Score;
        }
    }

    public class GameState : IEquatable<GameState>
    {
        public int HeroGold { get; set; }
        public int HeroHealth { get; set; }
        public int HeroWeapon { get; set; }

        // position : corner / side / center
        // neighbors : L/R/U/D -> monster / weapon / code /
        // look ahead: count of different types of cards of neighbors?

        public static GameState FromBoard(Board board)
        {
            return new GameState()
            {
                HeroGold = board.Gold,
                HeroHealth = board.HeroHealth,
                HeroWeapon = board.Weapon,
            };
        }

        public bool Equals(GameState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return HeroGold == other.HeroGold && HeroHealth == other.HeroHealth && HeroWeapon == other.HeroWeapon;
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
                var hashCode = HeroGold;
                hashCode = (hashCode * 397) ^ HeroHealth;
                hashCode = (hashCode * 397) ^ HeroWeapon;
                return hashCode;
            }
        }
    }

    public class MoveScore : IComparable<MoveScore>
    {
        public double Score { get; set; }

        public int CompareTo(MoveScore other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Score.CompareTo(other.Score);
        }

        public static MoveScore FromChangeInState(GameState oldState, GameState newState)
        {
            // TODO: Calc difference
            return new MoveScore(){Score = 0.5};
        }
    }

    public class Decision
    {
        public GameState State { get; set; }
        public SlotState SlotState { get; set; }
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
    }

    public interface IDecisionScorer
    {
        MoveScore GetScoresForState(Decision decision);
    }

    public interface IDecisionUpdater
    {
        void UpdateScores(Decision decision, MoveScore score);
    }

    public class DecisionScores : IDecisionScorer, IDecisionUpdater
    {
        private readonly IDictionary<Decision, MoveScore> _choices = new Dictionary<Decision, MoveScore>();

        public MoveScore GetScoresForState(Decision decision)
        {
            if (_choices.TryGetValue(decision, out MoveScore score))
            {
                return score;
            }

            return new MoveScore()
            {
                Score = 0.1,
            };
        }

        public void UpdateScores(Decision decision, MoveScore score)
        {
            /*
             * TODO:
             *   - update existing decision if present
             *   - add new decision otherwise
             */
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
            _accumulatedScores.Add(new Tuple<Decision, MoveScore>(_latestDecision, MoveScore.FromChangeInState(_latestDecision.State, newState) ));
            _latestDecision = null;
        }

        public void Train()
        {
            foreach (var score in _accumulatedScores)
            {
                _initialScores.UpdateScores(score.Item1, score.Item2);
            }
        }
    }
}