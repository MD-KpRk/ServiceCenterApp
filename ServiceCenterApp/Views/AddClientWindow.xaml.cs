using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddClientWindow : Window
    {
        public AddClientWindow(AddClientViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}