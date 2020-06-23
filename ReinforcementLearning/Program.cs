using System;
using System.Collections.Generic;
using Game;
using Game.Player;

namespace ReinforcementLearning
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            IGameAgent agent = new ReinforcementLearningGameAgent();
            Board board = GameBuilder.GetRandomStartBoard();
            GameRunner gameRunner = new GameRunner(agent, _ => {});
            gameRunner.RunGame(board);
            Console.ReadLine();
        }
    }

    public class ReinforcementLearningGameAgent : GameAgentBase
    {
        protected override double GetScore(Board board, ISlot<ICard<CardType>> slot)
        {
            return -1;
        }
    }

    public class GameState // TODO: How to capture the transverse nature of the board?
    {
        public int HeroGold { get; set; }
        public int HeroHealth { get; set; }
        public int HeroWeapon { get; set; }

        // position : corner / side / center
        // neighbors : L/R/U/D -> monster / weapon / code /
        // look ahead: count of different types of cards of neighbors?
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
    }

    public class Decision
    {
        public GameState State { get; set; }
        public Direction Direction { get; set; }
    }

    public class Trainer
    {
        IDictionary<Decision, MoveScore> _choices = new Dictionary<Decision, MoveScore>();

        Direction GetBestChoice(GameState currentState)
        {
            throw new NotImplementedException("Not implemented");
        }

        Direction UpdateChoices(IDictionary<Decision, MoveScore> newDecisions)
        {
            throw new NotImplementedException("Not implemented");
        }
    }
}