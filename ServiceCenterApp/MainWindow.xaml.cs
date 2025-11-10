using ServiceCenterApp.Services;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceCenterApp
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(ViewModel == null)
            {
                throw new NullReferenceException();
            }
            ViewModel.InitializeNavigation(MainFrame);
            ViewModel.StartNavigation();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                throw new NullReferenceException();
            }
            ViewModel.IsMenuVisible = !ViewModel.IsMenuVisible;
        }


    }
}