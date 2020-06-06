using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DungeonCardsGeneticAlgo;
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
        }
    }

    public class MainWindowViewModel
    {
        private IReadOnlyCollection<Slot> _slots;
        private readonly IList<Slot> _slotList = new List<Slot>();
        private readonly Board _board = GameBuilder.GetRandomStartBoard();

        public MainWindowViewModel()
        {
            _slots = new ReadOnlyObservableCollection<Slot>(new ObservableCollection<Slot>(_slotList));
        }

        public ICommand DoRun { get; } = new Command();
        public int Health { get; set; }
        public int Gold { get; set; }

        private int DoOneRun(GameAgentMultipliers multipliers)
        {
            GameBuilder.RandomizeBoardToStart(_board);
            var gameAgent = new GameAgent(multipliers);
            var gameRunner = new GameRunner(gameAgent.GetDirectionFromAlgo, _ => {});
            return gameRunner.RunGame(_board);
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

/*
Gold multipliers              38.2574, 35.0479, 32.4267
Monster w/ weapon multipliers 35.8533, -58.9001, -26.1292
Monster no weapon multipliers 4.2462, 2.1694, 2.4283
Weapon w/ weapon multipliers  -24.2076, -27.1800, -52.8834
Weapon no weapon multipliers  57.8609, 47.2535, 35.5468
 */
