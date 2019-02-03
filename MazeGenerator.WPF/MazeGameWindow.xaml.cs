using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MazeGameWindow.xaml
    /// </summary>
    public partial class MazeGameWindow
    {
        private readonly List<Key> _pressedKeys = new List<Key>();
        private MazeGameController Game { get; }

        public MazeGameWindow(MazeGameController gameController)
        {
            InitializeComponent();

            DataContext = gameController;
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
            if (!_pressedKeys.Contains(e.Key))
            {
                _pressedKeys.Add(e.Key);
                if (_pressedKeys.Count == 1)
                {
                    Game.KeyPressed(e.Key);
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);
        }
    }
}