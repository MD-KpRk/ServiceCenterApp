using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddServiceWindow : Window
    {
        public AddServiceViewModel ViewModel => (AddServiceViewModel)DataContext;

        public AddServiceWindow(AddServiceViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            Loaded += async (s, e) => await ViewModel.LoadServicesAsync();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedService == null)
            {
                MessageBox.Show("Выберите услугу.");
                return;
            }
            DialogResult = true;
        }
    }
}