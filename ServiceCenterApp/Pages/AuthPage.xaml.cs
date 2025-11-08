using ServiceCenterApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServiceCenterApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        AuthPageViewModel ViewModel;
        public AuthPage(AuthPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            ViewModel = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Login("testmock"))
                MessageBox.Show("Success");
            else
                MessageBox.Show("Grand Fail");
            
        }
    }
}
