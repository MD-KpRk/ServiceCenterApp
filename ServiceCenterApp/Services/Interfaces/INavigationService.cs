using ServiceCenterApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
        void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel;
        public void NavigateToRoleMainPage();
        void StartNavigation();
        void GoBack();
    }
}
