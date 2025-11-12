using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class MainPage : Page
    {
        private MainPageViewModel? ViewModel => DataContext as MainPageViewModel;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDashboardDataAsync();
            }
        }
    }
}