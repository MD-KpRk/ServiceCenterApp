using ServiceCenterApp.ViewModels;
using System.Windows.Controls;

namespace ServiceCenterApp.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}