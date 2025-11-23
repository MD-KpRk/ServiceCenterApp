using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class SupplyWindow : Window
    {
        public SupplyViewModel ViewModel => (SupplyViewModel)DataContext;

        public SupplyWindow()
        {
            InitializeComponent();
            DataContext = new SupplyViewModel();
        }
    }
}