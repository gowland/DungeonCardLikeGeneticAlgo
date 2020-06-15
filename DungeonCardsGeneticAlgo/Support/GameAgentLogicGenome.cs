using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSolver;
using GeneticSolver.Expressions;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo.Support
{

    public class GameAgentLogicGenome : ICloneable
    {
        public GameAgentLogicGenome ()
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
        public IExpression WeaponWhenPossessingWeaponScoreFunc { get; set; }

        public object Clone()
        {
            var clone = new GameAgentLogicGenome();

            GoldScoreMultiplier.CopyTo(clone.GoldScoreMultiplier,0);
            MonsterWhenPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenPossessingWeaponScoreMultiplier, 0);
            MonsterWhenNotPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenNotPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingNotWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingNotWeaponScoreMultiplier, 0);
            clone.WeaponWhenPossessingWeaponScoreFunc = (IExpression)WeaponWhenPossessingWeaponScoreFunc.Clone();

            return clone;
        }
    }
}