using ServiceCenterApp.Services.Interfaces;

namespace ServiceCenterApp.ViewModels
{
    public class AuthPageViewModel : BaseViewModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        private string _realPassword = "";
        public string StarPassword => new string('●', _realPassword.Length);

        public AuthPageViewModel(ICurrentUserService currentUserService,
                                 IAuthenticationService authenticationService,
                                 INavigationService navigationService)
        {
            _currentUserService = currentUserService;
            _authenticationService = authenticationService;
            _navigationService = navigationService; 
        }

        public void ProcessNumpadInput(string key)
        {
            switch (key)
            {
                case "Delete":
                    if (_realPassword.Length > 0)
                    {
                        _realPassword = _realPassword.Substring(0, _realPassword.Length - 1);
                    }
                    break;

                case "Apply":
                    Login();
                    break;

                default:
                    if (_realPassword.Length < 10) 
                    {
                        _realPassword += key;
                    }
                    break;
            }
            OnPropertyChanged(nameof(StarPassword));
        }

        private void Login()
        {
            if (string.IsNullOrEmpty(_realPassword)) return;
            _authenticationService.LoginAsync(_realPassword);

            _realPassword = "";
            OnPropertyChanged(nameof(StarPassword));
        }
    }
}