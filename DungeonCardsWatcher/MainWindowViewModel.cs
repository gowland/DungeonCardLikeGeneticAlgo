﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DungeonCardsGeneticAlgo.Support;
using DungeonCardsGeneticAlgo.Support.Multipliers;
using DungeonCardsGeneticAlgo.Support.WithLogic;
using DungeonCardsWatcher.Mvvm;
using Game;
using GeneticSolver.Expressions;
using GeneticSolver.Expressions.Implementations;

namespace DungeonCardsWatcher
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        private readonly IList<ViewSlot> _slotList = new List<ViewSlot>();
        private readonly Board _board = GameBuilder.GetRandomStartBoard();
        private bool _isRunningGame;
        private readonly Dispatcher _mainWindowDispatcher;
        private ObservableCollection<ViewSlot> _observableCollection;

        public MainWindowViewModel()
        {
            _observableCollection = new ObservableCollection<ViewSlot>(_slotList);
            Slots = new ReadOnlyObservableCollection<ViewSlot>(_observableCollection);

            DoRun = new Command(async _ => await DoOneRun(), _ =>true);
            _mainWindowDispatcher = Application.Current.MainWindow.Dispatcher;
        }

        public IReadOnlyCollection<ViewSlot> Slots { get; }
        public ICommand DoRun { get; }
        public int Health => _board?.HeroHealth ?? 0;
        public int Weapon => _board?.Weapon ?? 0;
        public int Gold => _board?.Gold ?? 0;
        public Direction? MoveDirection { get; set; }

        public bool IsRunningGame
        {
            get => _isRunningGame;
            set => SetProperty(ref _isRunningGame, value);
        }

        private async Task DoOneRun()
        {
            IsRunningGame = true;

            await Task.Run(DoLogicRun);

            IsRunningGame = false;
        }

        private int DoMultipliersRun()
        {
            var multipliers = new GameAgentMultipliers()
            {
                GoldScoreMultiplier = new double[3]{31.2382, 28.2065, 37.8986},
                MonsterWhenPossessingWeaponScoreMultiplier = new double[3]{34.6751, -51.6451, -32.8258},
                MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3]{2.9752, 4.7708, 3.2642},
                WeaponWhenPossessingWeaponScoreMultiplier = new double[3]{-18.5836, -8.9805, -39.5702},
                WeaponWhenPossessingNotWeaponScoreMultiplier = new double[3]{55.1838, 28.1914, 29.3060},
            };

            var agent = new GameAgent(multipliers);

            return LongRunningTask(agent);
        }

        private int DoLogicRun()
        {
            var multipliers = new GameAgentLogicGenome()
            {
                GoldScoreMultiplier = new double[3]{8.9333, 22.5057, 38.3460},
                MonsterWhenPossessingWeaponScoreMultiplier = new double[3]{10.7683, 10.3861, 3.7720},
                MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3]{-19.0546, -20.1365, -25.0921},
                WeaponWhenPossessingWeaponScoreMultiplier = new double[3]{-33.6411, -38.3365, -36.0864},
                WeaponWhenNotPossessingWeaponScoreMultiplier = new double[3]{22.2358, 38.3204, 14.1252},
                MonsterWhenPossessingWeaponScoreFunc = new ValueExpression<GameState>(-42.7745673178),
                MonsterWhenNotPossessingWeaponScoreFunc = new BoundValueExpression<GameState>(s => s.MonsterHealth, nameof(GameState.MonsterHealth)),
                WeaponWhenPossessingWeaponScoreFunc = new ValueExpression<GameState>(3.99947),
                WeaponWhenNotPossessingWeaponScoreFunc = new ValueExpression<GameState>(329.78287),
            };

            var agent = new GameAgentWithLogic(multipliers);

            return LongRunningTask(agent);
        }

        private int LongRunningTask(IGameAgent gameAgent)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            UpdateBoard(this, EventArgs.Empty);
            var gameRunner = new GameRunner(gameAgent.GetDirectionFromAlgo, _ => { });
            gameRunner.StateChanged += UpdateBoard;
            gameRunner.DirectionChosen += ShowDirection;
            int runResult = gameRunner.RunGame(_board);
            gameRunner.StateChanged -= UpdateBoard;
            gameRunner.DirectionChosen -= ShowDirection;
            return runResult;
        }

        private int LongRunningTask(GameAgentMultipliers multipliers)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            UpdateBoard(this, EventArgs.Empty);
            var gameAgent = new GameAgent(multipliers);
            var gameRunner = new GameRunner(gameAgent.GetDirectionFromAlgo, _ => { });
            gameRunner.StateChanged += UpdateBoard;
            gameRunner.DirectionChosen += ShowDirection;
            int runResult = gameRunner.RunGame(_board);
            gameRunner.StateChanged -= UpdateBoard;
            gameRunner.DirectionChosen -= ShowDirection;
            return runResult;
        }

        private void ShowDirection(object sender, Direction direction)
        {
            MoveDirection = direction;
            _mainWindowDispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                OnPropertyChanged(nameof(MoveDirection));
            }));

            Thread.Sleep(1000);
        }

        private void UpdateBoard(object sender, EventArgs args)
        {
            MoveDirection = null;
            var viewSlots =  _board.GetSlots().Select(s => new ViewSlot()
            {
                Type = ToViewCardType(s.Card.Type),
                Value = s.Card.Value
            });
            _mainWindowDispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                _observableCollection.Clear();
                foreach (var viewSlot in viewSlots)
                {
                    _observableCollection.Add(viewSlot);
                }

                OnPropertyChanged(nameof(MoveDirection));
                OnPropertyChanged(nameof(Health));
                OnPropertyChanged(nameof(Weapon));
                OnPropertyChanged(nameof(Gold));
            }));
            Thread.Sleep(1000);
        }

        private static ViewCardType ToViewCardType(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Monster:
                    return ViewCardType.Monster;
                case CardType.Weapon:
                    return ViewCardType.Weapon;
                case CardType.Player:
                    return ViewCardType.Hero;
                case CardType.Gold:
                    return ViewCardType.Gold;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cardType), cardType, null);
            }
        }
    }
}