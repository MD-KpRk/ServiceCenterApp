using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddTransactionWindow : Window
    {
        public AddTransactionWindow(AddTransactionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}