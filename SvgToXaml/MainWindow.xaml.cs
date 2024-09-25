using SvgToXaml.ViewModel;
using System.Windows;

namespace SvgToXaml
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new SvgToXamlViewModel();
        }
    }
}
