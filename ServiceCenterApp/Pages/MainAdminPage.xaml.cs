using ServiceCenterApp.Attributes;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ServiceCenterApp.Pages
{
    [RequiredPermission(PermissionEnum.Admin)]
    public partial class MainAdminPage : Page
    {
        private MainAdminPageViewModel? ViewModel => DataContext as MainAdminPageViewModel;

        public MainAdminPage(MainAdminPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.LoadDashboardDataAsync();
            }
        }
        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;

                PageScrollViewer.RaiseEvent(eventArg);
            }
        }
    }
}