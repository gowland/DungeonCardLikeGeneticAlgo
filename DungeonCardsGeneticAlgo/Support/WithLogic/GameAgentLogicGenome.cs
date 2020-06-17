using System;
using GeneticSolver.Expressions;

namespace DungeonCardsGeneticAlgo.Support.WithLogic
{

    public class GameAgentLogicGenome : ICloneable
    {
        public GameAgentLogicGenome ()
        {
            GoldScoreMultiplier = new double[3];
            MonsterWhenPossessingWeaponScoreMultiplier = new double[3];
            MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3];
            WeaponWhenPossessingWeaponScoreMultiplier = new double[3];
            WeaponWhenNotPossessingWeaponScoreMultiplier = new double[3];
        }
        public double[] GoldScoreMultiplier { get; set; }
        public double[] MonsterWhenPossessingWeaponScoreMultiplier { get; set; }
        public double[] MonsterWhenNotPossessingWeaponScoreMultiplier { get; set; }
        public double[] WeaponWhenPossessingWeaponScoreMultiplier { get; set; }
        public double[] WeaponWhenNotPossessingWeaponScoreMultiplier { get; set; }
        public IExpression<GameState> MonsterWhenPossessingWeaponScoreFunc { get; set; }
        public IExpression<GameState> MonsterWhenNotPossessingWeaponScoreFunc { get; set; }
        public IExpression<GameState> WeaponWhenPossessingWeaponScoreFunc { get; set; }
        public IExpression<GameState> WeaponWhenNotPossessingWeaponScoreFunc { get; set; }

        public object Clone()
        {
            var clone = new GameAgentLogicGenome();

            GoldScoreMultiplier.CopyTo(clone.GoldScoreMultiplier,0);
            MonsterWhenPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenPossessingWeaponScoreMultiplier, 0);
            MonsterWhenNotPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenNotPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingWeaponScoreMultiplier, 0);
            WeaponWhenNotPossessingWeaponScoreMultiplier.CopyTo(clone.WeaponWhenNotPossessingWeaponScoreMultiplier, 0);
            clone.MonsterWhenPossessingWeaponScoreFunc = (IExpression<GameState>)MonsterWhenPossessingWeaponScoreFunc.Clone();
            clone.MonsterWhenNotPossessingWeaponScoreFunc = (IExpression<GameState>)MonsterWhenNotPossessingWeaponScoreFunc.Clone();
            clone.WeaponWhenPossessingWeaponScoreFunc = (IExpression<GameState>)WeaponWhenPossessingWeaponScoreFunc.Clone();
            clone.WeaponWhenNotPossessingWeaponScoreFunc = (IExpression<GameState>)WeaponWhenNotPossessingWeaponScoreFunc.Clone();

            return clone;
        }
    }
}