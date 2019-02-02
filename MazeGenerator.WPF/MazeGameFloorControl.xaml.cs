using System.Windows.Controls;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MazeFloorControl.xaml
    /// </summary>
    public partial class MazeGameFloorControl : UserControl
    {
        public MazeGameFloorControl()
        {
            InitializeComponent();
        }

        internal MazeGameFloorControl(MazeFloorViewModel<MazeCellViewModel> vm) : this()
        {
            this.DataContext = vm;
        }
    }
}
