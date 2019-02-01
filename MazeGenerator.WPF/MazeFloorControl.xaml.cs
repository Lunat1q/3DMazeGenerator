using System.Windows.Controls;

namespace MazeGenerator.WPF
{
    /// <summary>
    /// Interaction logic for MazeFloorControl.xaml
    /// </summary>
    public partial class MazeFloorControl : UserControl
    {
        public MazeFloorControl()
        {
            InitializeComponent();
        }

        internal MazeFloorControl(MazeFloorViewModel vm) : this()
        {
            this.DataContext = vm;
        }
    }
}
