using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddEmployeeWindow : Window
    {
        public AddEmployeeViewModel ViewModel => (AddEmployeeViewModel)DataContext;
        public AddEmployeeWindow(AddEmployeeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDataAsync();
            }
        }
    }
}