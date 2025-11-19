using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddOrderWindow : Window
    {
        public AddOrderViewModel ViewModel => (AddOrderViewModel)DataContext;

        public AddOrderWindow(AddOrderViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync();
        }
    }
}