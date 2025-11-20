using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class EmployeesPage : Page
    {
        private EmployeesViewModel? ViewModel => DataContext as EmployeesViewModel;

        public EmployeesPage(EmployeesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += EmployeesPage_Loaded;
        }

        private async void EmployeesPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadInitialDataAsync();
            }
        }
    }
}