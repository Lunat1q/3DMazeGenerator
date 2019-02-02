using System.Windows.Controls;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MazeCell.xaml
    /// </summary>
    public partial class MazeCellControl : UserControl
    {
        public MazeCellControl()
        {
            InitializeComponent();
        }

        internal MazeCellControl(MazeCellViewModel vm) : this()
        {
            this.DataContext = vm;
        }
    }
}
