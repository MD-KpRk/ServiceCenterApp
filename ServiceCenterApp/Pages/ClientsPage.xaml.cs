using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class ClientsPage : Page
    {
        private ClientsViewModel? ViewModel => DataContext as ClientsViewModel;

        public ClientsPage(ClientsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += ClientsPage_Loaded;
        }

        private async void ClientsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null) await ViewModel.LoadInitialDataAsync();
        }
    }
}