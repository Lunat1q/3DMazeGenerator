using System.Windows.Controls;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MazeCell.xaml
    /// </summary>
    public partial class MazeGameCellControl : UserControl
    {
        public MazeGameCellControl()
        {
            InitializeComponent();
        }

        internal MazeGameCellControl(MazeCellViewModel vm) : this()
        {
            DataContext = vm;
        }
    }
}
