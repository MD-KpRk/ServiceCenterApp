using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddEditServiceWindow : Window
    {
        public AddEditServiceWindow(AddEditServiceViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}