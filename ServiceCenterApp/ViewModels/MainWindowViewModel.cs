using ServiceCenterApp.Services.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace ServiceCenterApp.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _title = "Сервисный Центр";
        private double _menuwidth = 50;
        private bool _isMenuExtended = false;
        private bool _isMenuVisible = false;

        private readonly INavigationService _navigationService;
        private readonly ICurrentUserService _currentUserService;

        public MainWindowViewModel(INavigationService navigationService, ICurrentUserService currentUserService)
        {
            _navigationService = navigationService;
            _currentUserService = currentUserService;

            _currentUserService.AuthenticationStateChanged += OnAuthenticationStateChanged;

            UpdateMenuVisibility();
        }

        public void InitializeNavigation(Frame frame)
        {
            _navigationService.Initialize(frame);
        }

        public void StartNavigation()
        {
            _navigationService.StartNavigation();
        }

        public bool IsMenuExtended
        {
            get => _isMenuExtended;
            set
            {
                _isMenuExtended = value;
                OnPropertyChanged(nameof(IsMenuExtended));
            }
        }

        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                _isMenuVisible = value;
                OnPropertyChanged(nameof(IsMenuVisible));
            }
        }

        public double MenuWidth
        {
            get => _menuwidth;
            set
            {
                _menuwidth = value;
                OnPropertyChanged(nameof(MenuWidth));
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }


        private void OnAuthenticationStateChanged()
        {
            UpdateMenuVisibility();
        }

        private void UpdateMenuVisibility()
        {
            IsMenuVisible = _currentUserService.IsLoggedIn;
        }
    }
}