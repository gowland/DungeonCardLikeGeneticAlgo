using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DungeonCardsGeneticAlgo;
using DungeonCardsGeneticAlgo.Support;
using DungeonCardsWatcher.Mvvm;
using Game;

namespace DungeonCardsWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }

    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        private IReadOnlyCollection<Slot> _slots;
        private readonly IList<Slot> _slotList = new List<Slot>();
        private readonly Board _board = GameBuilder.GetRandomStartBoard();
        private bool _isRunningGame;

        public MainWindowViewModel()
        {
            _slots = new ReadOnlyObservableCollection<Slot>(new ObservableCollection<Slot>(_slotList));
            var multipliers = new GameAgentMultipliers()
            {
                GoldScoreMultiplier = new double[3]{38.2574, 35.0479, 32.4267},
                MonsterWhenPossessingWeaponScoreMultiplier = new double[3]{35.8533, -58.9001, -26.1292},
                MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3]{4.2462, 2.1694, 2.4283},
                WeaponWhenPossessingWeaponScoreMultiplier = new double[3]{-24.2076, -27.1800, -52.8834},
                WeaponWhenPossessingNotWeaponScoreMultiplier = new double[3]{57.8609, 47.2535, 35.5468},
            };
            DoRun = new Command(_ => DoOneRun(multipliers), _ =>true);
        }

        public ICommand DoRun { get; }
        public int Health => _board?.HeroHealth ?? 0;
        public int Weapon => _board?.Weapon ?? 0;
        public int Gold => _board?.Gold ?? 0;

        public bool IsRunningGame
        {
            get => _isRunningGame;
            set => SetProperty(ref _isRunningGame, value);
        }

        private int DoOneRun(GameAgentMultipliers multipliers)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            UpdateBoard(this, EventArgs.Empty);
            var gameAgent = new GameAgent(multipliers);
            var gameRunner = new GameRunner(gameAgent.GetDirectionFromAlgo, _ => {});
            IsRunningGame = true;
            int? runResult = null;

            var dispatcher = Application.Current.MainWindow.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                gameRunner.StateChanged += UpdateBoard;
                gameRunner.DirectionChosen += ShowDirection;
                runResult = gameRunner.RunGame(_board);
                gameRunner.StateChanged -= UpdateBoard;
                gameRunner.DirectionChosen -= ShowDirection;
            }));

            IsRunningGame = false;
            return runResult ?? 0;
        }

        private void ShowDirection(object sender, Direction direction)
        {
            // TODO: Show selected direction
            Thread.Sleep(1000);
        }

        private void UpdateBoard(object sender, EventArgs args)
        {
            OnPropertyChanged(nameof(Health));
            OnPropertyChanged(nameof(Weapon));
            OnPropertyChanged(nameof(Gold));
            Thread.Sleep(1000);
        }
    }

    public class Slot
    {
        public CardType Type { get; set; }
        public int Value { get; set; }
    }

    public enum CardType
    {
        Hero,
        Monster,
        Weapon,
        Gold
    }
}

