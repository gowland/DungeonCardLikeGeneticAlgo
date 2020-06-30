using System;
using Game;
using Game.Player;

namespace DungeonCardsGeneticAlgo.Support.WithLogic
{
    public class GameAgentWithLogic : IGameAgent
    {
        private readonly IMoveSelector _itemSelector;
        private readonly GameAgentLogicGenome _multipliers;

        public GameAgentWithLogic(IMoveSelector itemSelector, GameAgentLogicGenome multipliers)
        {
            _itemSelector = itemSelector;
            _multipliers = multipliers;
        }

        protected double GetScore(Board board, ISlot<ICard<CardType>> slot)
        {
            var card = slot.Card;
            SquareDesc squareDesc = board.Desc();
                    var state = new GameState()
                    {
                        HeroGold = board.Gold,
                        HeroHealth = board.HeroHealth,
                        HeroWeapon = board.Weapon,
                        CardGold = 0,
                        MonsterHealth = 0,
                        CardWeapon = 0,
                    };
            switch (card.Type)
            {
                case CardType.Monster:
                    state.MonsterHealth = card.Value;
                    return ScoreMonsterCard(squareDesc, state);
                case CardType.Weapon:
                    state.CardWeapon = card.Value;
                    return ScoreWeaponCard(squareDesc, state);
                case CardType.Gold:
                    state.CardGold = card.Value;
                    return ScoreGoldCard(squareDesc, state);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double ScoreGoldCard(SquareDesc squareDesc, GameState state)
        {
            return _multipliers.GoldScoreMultiplier[(int)squareDesc] * state.CardGold;
        }

        private double ScoreWeaponCard(SquareDesc squareDesc, GameState state)
        {
            return state.HeroWeapon > 0
                ? _multipliers.WeaponWhenPossessingWeaponScoreMultiplier[(int) squareDesc] * _multipliers.WeaponWhenPossessingWeaponScoreFunc.Evaluate(state)
                : _multipliers.WeaponWhenNotPossessingWeaponScoreMultiplier[(int) squareDesc] * _multipliers.WeaponWhenNotPossessingWeaponScoreFunc.Evaluate(state);
        }

        private double ScoreMonsterCard(SquareDesc squareDesc, GameState state)
        {
            if (state.HeroWeapon > 0)
            {
                return _multipliers.MonsterWhenPossessingWeaponScoreMultiplier[(int)squareDesc] * _multipliers.MonsterWhenPossessingWeaponScoreFunc.Evaluate(state);
            }
            else if (state.MonsterHealth > state.HeroHealth)
            {
                return ScoreMonsterWhenNotPossessingWeaponAndMonsterHealthIsGreater(state);
            }
            else
            {
                return _multipliers.MonsterWhenNotPossessingWeaponScoreMultiplier[(int)squareDesc] * _multipliers.MonsterWhenNotPossessingWeaponScoreFunc.Evaluate(state);
            }
        }

        private double ScoreMonsterWhenNotPossessingWeaponAndMonsterHealthIsGreater(GameState state)
        {
            return -100;
        }

        public DirectionResult GetDirectionFromAlgo(Board board)
        {
            return _itemSelector.GetDirectionFromAlgo(board, this.GetScore);
        }
    }
}