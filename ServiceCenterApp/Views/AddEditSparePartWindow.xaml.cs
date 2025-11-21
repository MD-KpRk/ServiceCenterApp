using ServiceCenterApp.ViewModels;
using System.Windows;

namespace ServiceCenterApp.Views
{
    public partial class AddEditSparePartWindow : Window
    {
        // Конструктор, который принимает ViewModel
        public AddEditSparePartWindow(AddEditSparePartViewModel viewModel)
        {
            InitializeComponent();

            // Устанавливаем DataContext, чтобы привязки в XAML работали
            DataContext = viewModel;
        }
    }
}