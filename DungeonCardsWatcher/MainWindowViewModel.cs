using System;
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
using Game.Player;
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

            DoRun = new Command(async _ => await DoOneRun(DoMultipliersRun), _ =>true);
            DoSmartRun = new Command(async _ => await DoOneRun(DoLogicRun), _ =>true);
            _mainWindowDispatcher = Application.Current.MainWindow.Dispatcher;
        }

        public IReadOnlyCollection<ViewSlot> Slots { get; }
        public ICommand DoRun { get; }
        public ICommand DoSmartRun { get; }
        public int Health => _board?.HeroHealth ?? 0;
        public int Weapon => _board?.Weapon ?? 0;
        public int Gold => _board?.Gold ?? 0;
        public Direction? MoveDirection { get; set; }

        public bool IsRunningGame
        {
            get => _isRunningGame;
            set => SetProperty(ref _isRunningGame, value);
        }

        private async Task DoOneRun(Action run)
        {
            IsRunningGame = true;

            await Task.Run(run);

            IsRunningGame = false;
        }

        private void DoMultipliersRun()
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

            LongRunningTask(agent);
        }

        private void DoLogicRun()
        {
            var expressionGenerator = ExpressionGeneratorFactory.CreateExpressionGenerator();

            var multipliers = new GameAgentLogicGenome()
            {
                GoldScoreMultiplier = new double[3]{25.5316, 24.0099, 16.2610},
                MonsterWhenPossessingWeaponScoreMultiplier = new double[3]{24.5933, 23.4264, 22.0143},
                MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3]{-31.2997, -22.0128, -29.7514},
                WeaponWhenPossessingWeaponScoreMultiplier = new double[3]{-43.3635, -21.5867, -27.6219},
                WeaponWhenNotPossessingWeaponScoreMultiplier = new double[3]{-43.2861, -20.3950, -29.5461},
                MonsterWhenPossessingWeaponScoreFunc = expressionGenerator.FromString("((((HeroGold) - ((((HeroWeapon) - ((HeroWeapon) + ((-07.11710) + (19.00022)))) - (18.00540)) - (CardWeapon))) + ((-14.95005) * (((-01.94737) - (04.02547)) - (CardWeapon)))) * (((((HeroWeapon) + (HeroHealth)) * ((HeroHealth) * (((-00.41577) - (06.60667)) * ((12.55579) + (07.71862))))) * (((CardWeapon) * (-18.98748)) * (((-02.64072) + ((MonsterHealth) - ((CardGold) + (MonsterHealth)))) * ((-06.30514) + (14.90779))))) + (((((-15.86993) - (MonsterHealth)) + ((-14.95005) * (((-05.25034) - (08.87666)) - (CardWeapon)))) * (((HeroHealth) + (((04.39729) - (11.29765)) + (-14.68780))) * (((-18.98172) - (-09.60717)) * (07.72093)))) + (((HeroWeapon) + (HeroHealth)) * ((HeroHealth) + (((00.06431) - ((-14.26864) - ((-17.26604) + (CardGold)))) + ((CardWeapon) + (12.43572)))))))) + ((HeroGold) + (((CardWeapon) + ((-14.95005) * (((-05.25034) - (12.70156)) - (CardWeapon)))) + (((HeroGold) - (MonsterHealth)) - ((-18.77707) * (((-05.25034) - (-10.68887)) - (CardWeapon))))))"),
                MonsterWhenNotPossessingWeaponScoreFunc = expressionGenerator.FromString("(((MonsterHealth) + (10.36471)) + (-08.94748)) + (((MonsterHealth) + (11.08993)) - (((MonsterHealth) + (12.71245)) - (CardGold)))"),
                WeaponWhenPossessingWeaponScoreFunc = expressionGenerator.FromString("((((19.58677) - (00.69406)) - (-06.99937)) * (((((19.18323) * (HeroWeapon)) + (((((-01.64314) - ((MonsterHealth) * (05.89712))) + (((-06.60712) - (((11.88549) + ((((MonsterHealth) + (-02.58400)) + ((04.93510) - ((19.22972) * (CardGold)))) + (HeroWeapon))) - (CardGold))) + (HeroHealth))) * ((HeroGold) - ((HeroWeapon) * ((03.31325) - (06.59709))))) * (-10.23020))) * ((((HeroHealth) * ((CardWeapon) * (13.90223))) - ((18.60329) + (CardGold))) - ((12.86722) + ((((CardGold) - (CardWeapon)) * (-00.79672)) + (-06.02827))))) + (((((HeroHealth) - (00.69406)) - (MonsterHealth)) - ((12.26878) * ((CardGold) * ((((HeroHealth) * (HeroHealth)) - (HeroHealth)) + (-05.28352))))) * (((19.18323) * (HeroWeapon)) + (((((-01.64314) - ((MonsterHealth) * (05.89712))) + (((-06.60712) - (((11.88549) + ((((MonsterHealth) + (-02.58400)) + ((04.93510) - ((19.22972) * (CardGold)))) + (HeroWeapon))) - (CardGold))) + (HeroHealth))) * ((HeroGold) - ((HeroWeapon) * ((03.31325) - (06.59709))))) * (-10.23020)))))) - ((((HeroHealth) - ((-03.41793) + (((MonsterHealth) + (HeroHealth)) - (-00.95787)))) - (CardGold)) * ((((HeroHealth) - (05.21138)) - (CardGold)) + (((HeroHealth) - ((-22.22169) * (CardWeapon))) - (CardGold))))"),
                WeaponWhenNotPossessingWeaponScoreFunc = expressionGenerator.FromString("(-11.46160) + (((-17.80806) + (CardWeapon)) + (((-17.80806) + (CardWeapon)) + ((-17.80806) + ((-05.75740) - (HeroHealth)))))"),
            };

            var agent = new GameAgentWithLogic(multipliers);

            LongRunningTask(agent);
        }

        private void LongRunningTask(IGameAgent gameAgent)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            UpdateBoard(this, EventArgs.Empty);
            var gameRunner = new GameRunner(gameAgent, _ => { });
            gameRunner.StateChanged += UpdateBoard;
            gameRunner.DirectionChosen += ShowDirection;
            int runResult = gameRunner.RunGame(_board);
            gameRunner.StateChanged -= UpdateBoard;
            gameRunner.DirectionChosen -= ShowDirection;
        }

        /*
        private void LongRunningTask(GameAgentMultipliers multipliers)
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
        }
        */

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
            Thread.Sleep(2000);
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