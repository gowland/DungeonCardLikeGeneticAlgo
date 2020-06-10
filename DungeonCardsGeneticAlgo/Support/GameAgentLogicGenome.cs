using System;
using System.Collections.Generic;

namespace DungeonCardsGeneticAlgo.Support
{
    public interface IExpression
    {
        double Evaluate();
        void Mutate();
    }

    public class ValueExpression : IExpression
    {
        private double _value;

        public ValueExpression(double value)
        {
            _value = value;
        }
        public double Evaluate()
        {
            return _value;
        }

        public void Mutate()
        {
            // Change value within range
        }
    }

    public class BoundValueExpression : IExpression
    {
        private readonly Func<double> _getValueFunc;

        public BoundValueExpression(Func<double> getValueFunc)
        {
            _getValueFunc = getValueFunc;
        }
        public double Evaluate()
        {
            return _getValueFunc();
        }

        public void Mutate()
        {
            // No action
        }
    }

    public class FuncExpression : IExpression
    {
        public IExpression Left { get; set; }
        public IExpression Right { get; set; }
        public Func<double, double, double> Operation { get; set; }

        public double Evaluate()
        {
            return Operation(Left.Evaluate(), Right.Evaluate());
        }

        public void Mutate()
        {
            // Swap left, swap right, swap operation
            // Mutate left, mutate right
        }
    }

    public interface IExpressionSource
    {
        IEnumerable<IExpression> Expressions { get; set; }
    }

    public interface IExpressionValueSource : IExpressionSource
    {
    }

    public interface IExpressionFuncSource : IExpressionSource
    {
    }

    public class MutatableFunction
    {
        private readonly IExpressionValueSource _valueSource;
        private readonly IExpressionFuncSource _funcSource;

        public MutatableFunction(IExpressionValueSource valueSource, IExpressionFuncSource funcSource)
        {
            _valueSource = valueSource;
            _funcSource = funcSource;
        }

        public void Mutate()
        {

        }
    }

    public class GameAgentLogicGenome : ICloneable
    {
        public GameAgentLogicGenome ()
        {
            GoldScoreMultiplier = new double[3];
            MonsterWhenPossessingWeaponScoreMultiplier = new double[3];
            MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3];
            WeaponWhenPossessingWeaponScoreMultiplier = new double[3];
            WeaponWhenPossessingNotWeaponScoreMultiplier = new double[3];

            WeaponWhenPossessingWeaponScoreFunc = (heroWeapon, cardWeapon, heroHealth) => cardWeapon - heroWeapon;
        }
        public double[] GoldScoreMultiplier { get; set; }
        public double[] MonsterWhenPossessingWeaponScoreMultiplier { get; set; }
        public double[] MonsterWhenNotPossessingWeaponScoreMultiplier { get; set; }
        public double[] WeaponWhenPossessingWeaponScoreMultiplier { get; set; }
        public double[] WeaponWhenPossessingNotWeaponScoreMultiplier { get; set; }
        public object Clone()
        {
            var clone = new GameAgentMultipliers();

            GoldScoreMultiplier.CopyTo(clone.GoldScoreMultiplier,0);
            MonsterWhenPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenPossessingWeaponScoreMultiplier, 0);
            MonsterWhenNotPossessingWeaponScoreMultiplier.CopyTo(clone.MonsterWhenNotPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingWeaponScoreMultiplier, 0);
            WeaponWhenPossessingNotWeaponScoreMultiplier.CopyTo(clone.WeaponWhenPossessingNotWeaponScoreMultiplier, 0);

            return clone;
        }

        public Func<int, int, int, double> WeaponWhenPossessingWeaponScoreFunc { get; set; }
    }
}