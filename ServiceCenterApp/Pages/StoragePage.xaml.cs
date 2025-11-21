using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class StoragePage : Page
    {
        private StorageViewModel? ViewModel => DataContext as StorageViewModel;

        public StoragePage(StorageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (s, e) => ViewModel?.LoadDataAsync();
        }
    }
}