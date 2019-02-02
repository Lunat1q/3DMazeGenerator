using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MazeGameWindow.xaml
    /// </summary>
    public partial class MazeGameWindow : Window
    {
        private List<Key> pressedKeys = new List<Key>();
        private MazeGameController Game { get; }
        public MazeGameWindow(MazeGameController gameController)
        {
            InitializeComponent();

            this.DataContext = gameController;
            Game = gameController;

            InitGame();
        }

        private void InitGame()
        {
            Game.Prepare();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Game.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!pressedKeys.Contains(e.Key))
            {
                pressedKeys.Add(e.Key);
                if (pressedKeys.Count == 1)
                {
                    Game.KeyPressed(e.Key);
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.Key);
        }
    }
}
