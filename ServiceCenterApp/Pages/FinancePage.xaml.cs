using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class FinancePage : Page
    {
        private FinanceViewModel? ViewModel => DataContext as FinanceViewModel;

        public FinancePage(FinanceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += FinancePage_Loaded;
        }

        private async void FinancePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.RefreshAsync();
            }
        }
    }
}