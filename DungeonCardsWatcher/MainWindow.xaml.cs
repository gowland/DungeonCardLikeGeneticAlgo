using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private readonly IList<ViewSlot> _slotList = new List<ViewSlot>();
        private readonly Board _board = GameBuilder.GetRandomStartBoard();
        private bool _isRunningGame;
        private readonly Dispatcher _mainWindowDispatcher;
        private ObservableCollection<ViewSlot> _observableCollection;

        public MainWindowViewModel()
        {
            _observableCollection = new ObservableCollection<ViewSlot>(_slotList);
            Slots = new ReadOnlyObservableCollection<ViewSlot>(_observableCollection);
            var multipliers = new GameAgentMultipliers()
            {
                GoldScoreMultiplier = new double[3]{31.2382, 28.2065, 37.8986},
                MonsterWhenPossessingWeaponScoreMultiplier = new double[3]{34.6751, -51.6451, -32.8258},
                MonsterWhenNotPossessingWeaponScoreMultiplier = new double[3]{2.9752, 4.7708, 3.2642},
                WeaponWhenPossessingWeaponScoreMultiplier = new double[3]{-18.5836, -8.9805, -39.5702},
                WeaponWhenPossessingNotWeaponScoreMultiplier = new double[3]{55.1838, 28.1914, 29.3060},
            };
            DoRun = new Command(async _ => await DoOneRun(multipliers), _ =>true);
            _mainWindowDispatcher = Application.Current.MainWindow.Dispatcher;
        }

        public IReadOnlyCollection<ViewSlot> Slots { get; }
        public ICommand DoRun { get; }
        public int Health => _board?.HeroHealth ?? 0;
        public int Weapon => _board?.Weapon ?? 0;
        public int Gold => _board?.Gold ?? 0;

        public bool IsRunningGame
        {
            get => _isRunningGame;
            set => SetProperty(ref _isRunningGame, value);
        }

        private async Task DoOneRun(GameAgentMultipliers multipliers)
        {
            IsRunningGame = true;

            await Task.Run(()=> LongRunningTask(multipliers));

            IsRunningGame = false;
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
            // TODO: Show selected direction
            Thread.Sleep(1000);
        }

        private void UpdateBoard(object sender, EventArgs args)
        {
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

    public class ViewSlot
    {
        public ViewCardType Type { get; set; }
        public int Value { get; set; }
    }

    public enum ViewCardType
    {
        Hero,
        Monster,
        Gold,
        Weapon,
    }

}

