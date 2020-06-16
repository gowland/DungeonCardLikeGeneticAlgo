using System.Collections.Generic;
using GeneticSolver;
using GeneticSolver.Expressions;
using GeneticSolver.GenomeProperty;
using GeneticSolver.RequiredInterfaces;

namespace DungeonCardsGeneticAlgo.Support.WithLogic
{
    public class GameAgentLogicGenomeDescription : IGenomeDescription<GameAgentLogicGenome>
    {
        private readonly IRandom _random;
        private readonly ExpressionGenerator<GameState> _generator;
        private readonly double _minChange = -15;
        private readonly double _maxChange = 15;
        private int _minMultiplierValue = -100;
        private int _maxMultiplierValue = 100;

        public GameAgentLogicGenomeDescription(IRandom random, ExpressionGenerator<GameState> generator)
        {
            _random = random;
            _generator = generator;
        }

        public IEnumerable<IGenomeProperty<GameAgentLogicGenome>> Properties => new IGenomeProperty<GameAgentLogicGenome>[]
        {
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.GoldScoreMultiplier[0],
                (g, val) => g.GoldScoreMultiplier[0] = val, _minMultiplierValue, _maxMultiplierValue, _minChange,
                _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.GoldScoreMultiplier[1],
                (g, val) => g.GoldScoreMultiplier[1] = val, _minMultiplierValue, _maxMultiplierValue, _minChange,
                _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.GoldScoreMultiplier[2],
                (g, val) => g.GoldScoreMultiplier[2] = val, _minMultiplierValue, _maxMultiplierValue, _minChange,
                _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.MonsterWhenPossessingWeaponScoreMultiplier[0],
                (g, val) => g.MonsterWhenPossessingWeaponScoreMultiplier[0] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.MonsterWhenPossessingWeaponScoreMultiplier[1],
                (g, val) => g.MonsterWhenPossessingWeaponScoreMultiplier[1] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.MonsterWhenPossessingWeaponScoreMultiplier[2],
                (g, val) => g.MonsterWhenPossessingWeaponScoreMultiplier[2] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.MonsterWhenNotPossessingWeaponScoreMultiplier[0],
                (g, val) => g.MonsterWhenNotPossessingWeaponScoreMultiplier[0] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.MonsterWhenNotPossessingWeaponScoreMultiplier[1],
                (g, val) => g.MonsterWhenNotPossessingWeaponScoreMultiplier[1] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.MonsterWhenNotPossessingWeaponScoreMultiplier[2],
                (g, val) => g.MonsterWhenNotPossessingWeaponScoreMultiplier[2] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.WeaponWhenPossessingWeaponScoreMultiplier[0], (g, val) => g.WeaponWhenPossessingWeaponScoreMultiplier[0] = val, _minMultiplierValue, _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.WeaponWhenPossessingWeaponScoreMultiplier[1], (g, val) => g.WeaponWhenPossessingWeaponScoreMultiplier[1] = val, _minMultiplierValue, _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.WeaponWhenPossessingWeaponScoreMultiplier[2], (g, val) => g.WeaponWhenPossessingWeaponScoreMultiplier[2] = val, _minMultiplierValue, _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.WeaponWhenPossessingNotWeaponScoreMultiplier[0],
                (g, val) => g.WeaponWhenPossessingNotWeaponScoreMultiplier[0] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.WeaponWhenPossessingNotWeaponScoreMultiplier[1],
                (g, val) => g.WeaponWhenPossessingNotWeaponScoreMultiplier[1] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new DoubleGenomeProperty<GameAgentLogicGenome>(g => g.WeaponWhenPossessingNotWeaponScoreMultiplier[2],
                (g, val) => g.WeaponWhenPossessingNotWeaponScoreMultiplier[2] = val, _minMultiplierValue,
                _maxMultiplierValue, _minChange, _maxChange, _random),
            new ExpressionGenomeProperty<GameAgentLogicGenome, GameState>(g => g.WeaponWhenPossessingWeaponScoreFunc,
                (g, val) => g.WeaponWhenPossessingWeaponScoreFunc = val, _generator),
            new ExpressionGenomeProperty<GameAgentLogicGenome, GameState>(g => g.WeaponWhenNotPossessingWeaponScoreFunc,
                (g, val) => g.WeaponWhenNotPossessingWeaponScoreFunc = val, _generator),
        };
    }
}