using ServiceCenterApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ServiceCenterApp.Pages
{
    public partial class AuthPage : Page
    {
        private AuthPageViewModel? ViewModel => DataContext as AuthPageViewModel;

        public AuthPage(AuthPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string key)
            {
                ViewModel?.ProcessNumpadInput(key);
            }
        }
        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string? keyInput = e.Key switch
            {
                Key.D0 or Key.NumPad0 => "0",
                Key.D1 or Key.NumPad1 => "1",
                Key.D2 or Key.NumPad2 => "2",
                Key.D3 or Key.NumPad3 => "3",
                Key.D4 or Key.NumPad4 => "4",
                Key.D5 or Key.NumPad5 => "5",
                Key.D6 or Key.NumPad6 => "6",
                Key.D7 or Key.NumPad7 => "7",
                Key.D8 or Key.NumPad8 => "8",
                Key.D9 or Key.NumPad9 => "9",
                Key.Back => "Delete",
                Key.Enter => "Apply",
                _ => null
            };

            if (keyInput != null)
            {
                ViewModel?.ProcessNumpadInput(keyInput);
                e.Handled = true;
            }
        }
    }
}