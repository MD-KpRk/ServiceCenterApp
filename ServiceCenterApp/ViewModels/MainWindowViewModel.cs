using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Services.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public ICommand NavigateCommand { get; }

        public MainWindowViewModel(INavigationService navigationService, ICurrentUserService currentUserService)
        {
            _navigationService = navigationService;
            _currentUserService = currentUserService;

            _currentUserService.AuthenticationStateChanged += OnAuthenticationStateChanged;

            NavigateCommand = new RelayCommand<string>(Navigate);

            UpdateMenuAndPermissions();
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

        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Dashboard":
                    _navigationService.NavigateTo<MainAdminPageViewModel>();
                    break;
                case "Orders":
                    _navigationService.NavigateTo<OrdersViewModel>();
                    break;
                case "Clients":
                    _navigationService.NavigateTo<ClientsViewModel>();
                    break;

                    //TODO:
                    // Сюда можно добавить другие кейсы: "Clients", "Employees" и т.д.
            }
        }

        public Visibility DashBoardVisibility
        {
            get
            {
                if (_currentUserService.HasAllPermissions([PermissionEnum.Admin])) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility OrdersVisibility
        {
            get
            {
                if (_currentUserService.HasAllPermissions([PermissionEnum.Admin])) return Visibility.Visible;

                if (_currentUserService.HasAllPermissions([PermissionEnum.Orders])) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility ClientsVisibility
        {
            get
            {
                if (_currentUserService.HasAllPermissions([PermissionEnum.Admin])) return Visibility.Visible;

                if (_currentUserService.HasAllPermissions([PermissionEnum.Clients])) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility EmployeesVisibility
        {
            get
            {
                if (_currentUserService.HasAllPermissions([PermissionEnum.Admin])) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public Visibility StorageVisibility
        {
            get
            {
                if (_currentUserService.HasAllPermissions([PermissionEnum.Admin])) return Visibility.Visible;

                if (_currentUserService.HasAllPermissions([PermissionEnum.SparePart])) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility FinanceVisibility
        {
            get
            {
                if (_currentUserService.HasAllPermissions([PermissionEnum.Admin])) return Visibility.Visible;

                if (_currentUserService.HasAllPermissions([PermissionEnum.Payment])) return Visibility.Visible;
                return Visibility.Collapsed;
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
            UpdateMenuAndPermissions();
        }

        private void UpdateMenuAndPermissions()
        {
            IsMenuVisible = _currentUserService.IsLoggedIn;

            OnPropertyChanged(nameof(DashBoardVisibility));
            OnPropertyChanged(nameof(OrdersVisibility));
            OnPropertyChanged(nameof(ClientsVisibility));
            OnPropertyChanged(nameof(EmployeesVisibility));
            OnPropertyChanged(nameof(StorageVisibility));
            OnPropertyChanged(nameof(FinanceVisibility));
        }


    }
}