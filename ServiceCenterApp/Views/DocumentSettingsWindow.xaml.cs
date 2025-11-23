using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class DocumentSettingsWindow : Window
    {
        public DocumentSettingsViewModel ViewModel => (DocumentSettingsViewModel)DataContext;

        public DocumentSettingsWindow(int currentDays)
        {
            InitializeComponent();
            DataContext = new DocumentSettingsViewModel(currentDays);
        }
    }
}