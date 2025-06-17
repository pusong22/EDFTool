using EDFToolApp.ViewModel;
using System.Windows.Controls;

namespace EDFToolApp.View
{
    /// <summary>
    /// Interaction logic for StartupView.xaml
    /// </summary>
    public partial class StartupView : UserControl
    {
        public StartupView(StartupViewModel startupViewModel)
        {
            InitializeComponent();

            DataContext = startupViewModel;
        }
    }
}
