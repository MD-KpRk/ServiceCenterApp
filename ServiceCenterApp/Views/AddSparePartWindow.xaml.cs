using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddSparePartWindow : Window
    {
        // Публичное свойство для доступа к ViewModel извне
        public AddSparePartViewModel ViewModel => (AddSparePartViewModel)DataContext;

        public AddSparePartWindow(AddSparePartViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // await ViewModel.LoadSparePartsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedSparePart != null)
            {
                DialogResult = true; // Устанавливаем результат и закрываем окно
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запчасть из списка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}