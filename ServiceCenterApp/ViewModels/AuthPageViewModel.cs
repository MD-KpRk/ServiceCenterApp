using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ServiceCenterApp.ViewModels
{
    public class AuthPageViewModel : BaseViewModel
    {
        ICurrentUserService? _currentUserService;
        IAuthenticationService? _authenticationService;

        public AuthPageViewModel(ICurrentUserService currentUserService, IAuthenticationService authenticationService)
        {
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;

        }

        public bool Login(string password)
        {
            bool? result = _authenticationService?.Login("username", password);

            //MessageBox.Show(_currentUserService.CurrentUser);

            if (result == null) result = false;

            if (result == false) return false;
            else return true;

        }
    }
}
