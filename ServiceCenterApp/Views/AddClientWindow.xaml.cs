using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddClientWindow : Window
    {
        public AddClientViewModel ViewModel => (AddClientViewModel)DataContext;

        public AddClientWindow(AddClientViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}