using System;
using System.Linq;
using System.Threading;
using DungeonCardsGeneticAlgo.Support.Multipliers;
using DungeonCardsGeneticAlgo.Support.WithLogic;
using Game;
using Game.Player;

namespace DungeonCardsGeneticAlgo.Support
{
    public class GameAgent : IGameAgent
    {
        private readonly GameAgentBase _itemSelector;
        private readonly GameAgentMultipliers _multipliers;

        public GameAgent(GameAgentBase itemSelector, GameAgentMultipliers multipliers)
        {
            _itemSelector = itemSelector;
            _multipliers = multipliers;
        }

        protected  double GetScore(Board board, ISlot<ICard<CardType>> slot)
        {
            var card = slot.Card;
            SquareDesc squareDesc = board.Desc();
            switch (card.Type)
            {
                case CardType.Monster:
                    return ScoreMonsterCard(board.Weapon, card.Value, board.HeroHealth, squareDesc);
                case CardType.Weapon:
                    return ScoreWeaponCard(board.Weapon, card.Value, squareDesc);
                case CardType.Gold:
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
            return -100;
        }

        private double ScoreMonsterWhenNotPossessingWeaponAndHeroHealthIsGreater(int heroHealth, int monsterHealth)
        {
            return (heroHealth - monsterHealth);
        }

        private double ScoreMonsterWhenPossessingWeapon(int heroWeapon, int monsterHealth)
        {
            return (monsterHealth - heroWeapon);
        }

        public DirectionResult GetDirectionFromAlgo(Board board)
        {
            return _itemSelector.GetDirectionFromAlgo(board, this.GetScore);
        }
    }
}