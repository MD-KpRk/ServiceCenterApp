using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class OrdersPage : Page
    {
        private OrdersViewModel? ViewModel => DataContext as OrdersViewModel;

        public OrdersPage(OrdersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += OrdersPage_Loaded;
        }

        private async void OrdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadInitialDataAsync();
            }
        }
    }
}