using DungeonCardsGeneticAlgo;

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

