using System;
using System.Linq;
using Game;

namespace DungeonCardsGeneticAlgo.Support.WithLogic
{
    public class GameAgentWithLogic : IGameAgent
    {
        private readonly GameAgentLogicGenome _multipliers;

        public GameAgentWithLogic(GameAgentLogicGenome multipliers)
        {
            _multipliers = multipliers;
        }

        public DirectionResult GetDirectionFromAlgo(Board board)
        {
            var moves = board.GetCurrentLegalMoves();

            var scoredMoves = moves.Select(pair => new { Direction = pair.Key, Score = GetScore(board, pair.Value) });

            return new DirectionResult(scoredMoves.OrderByDescending(move => move.Score).First().Direction);
        }

        private double GetScore(Board board, ISlot<ICard<CardType>> slot)
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
                    return ScoreMonsterCard(board.Weapon, card.Value, board.HeroHealth, squareDesc);
                case CardType.Weapon:
                    state.CardWeapon = card.Value;
                    return ScoreWeaponCard(board.Weapon, card.Value, board.HeroHealth, squareDesc, state);
                case CardType.Gold:
                    state.CardGold = card.Value;
                    return ScoreGoldCard(card.Value, squareDesc);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double ScoreGoldCard(int goldScore, SquareDesc squareDesc)
        {
            return _multipliers.GoldScoreMultiplier[(int)squareDesc] * goldScore;
        }

        private double ScoreWeaponCard(int heroWeapon, int weaponValue, int heroHealth, SquareDesc squareDesc, GameState state)
        {
            return heroWeapon > 0
                ? _multipliers.WeaponWhenPossessingWeaponScoreMultiplier[(int) squareDesc] * _multipliers.WeaponWhenPossessingWeaponScoreFunc.Evaluate(state)
                : _multipliers.WeaponWhenPossessingNotWeaponScoreMultiplier[(int) squareDesc] * _multipliers.WeaponWhenNotPossessingWeaponScoreFunc.Evaluate(state);
        }

        private double ScoreWeaponCardWhenNotPossessingWeapon(int weaponValue)
        {
            return weaponValue;
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
    }
}