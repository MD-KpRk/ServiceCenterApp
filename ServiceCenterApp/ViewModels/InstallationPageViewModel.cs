using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class InstallationPageViewModel : BaseViewModel
    {
        private readonly IAuthenticationService? _authenticationService;
        private string? _firstName;
        private string? _surName;
        private string? _patronymic;
        private string? _position;
        private string? _pin1;
        private string? _pin2;

        public string? FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(nameof(FirstName)); }
        }

        public string? SurName
        {
            get => _surName;
            set { _surName = value; OnPropertyChanged(nameof(SurName)); }
        }

        public string? Patronymic
        {
            get => _patronymic;
            set { _patronymic = value; OnPropertyChanged(nameof(Patronymic)); }
        }

        public string? Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public string? Pin1
        {
            get => _pin1;
            set { _pin1 = value; OnPropertyChanged(nameof(Pin1)); }
        }

        public string? Pin2
        {
            get => _pin2;
            set { _pin2 = value; OnPropertyChanged(nameof(Pin2)); }
        }

        public ICommand CreateAdminCommand { get; }

        public int MaxFirstNameLength { get; }
        public int MaxSurNameLength { get; }
        public int MaxPatronymicLength { get; }
        public int MaxPositionLength { get; }

        public InstallationPageViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;

            MaxFirstNameLength = ModelHelper.GetMaxLength(typeof(Employee), nameof(Employee.FirstName));
            MaxSurNameLength = ModelHelper.GetMaxLength(typeof(Employee), nameof(Employee.SurName));
            MaxPatronymicLength = ModelHelper.GetMaxLength(typeof(Employee), nameof(Employee.Patronymic));
            MaxPositionLength = ModelHelper.GetMaxLength(typeof(Position), nameof(Models.Position.PositionName));
            CreateAdminCommand = new RelayCommand(CreateAdministrator, CanCreateAdministrator);
        }


        private bool CanCreateAdministrator(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(SurName) &&
                   !string.IsNullOrWhiteSpace(Position) &&
                   !string.IsNullOrWhiteSpace(Pin1) &&
                   Pin1 == Pin2;
        }

        public void ClearAll()
        {
            FirstName = SurName = Patronymic = Position = Pin1 = Pin2 = null;
        }

        private async void CreateAdministrator(object? parameter)
        {
            if (_authenticationService == null) throw new NullReferenceException("Auth Service not found");

            if (FirstName == null || SurName == null || Position == null || Pin1 == null) return;

            await _authenticationService.CreateAdministratorAsync(FirstName, SurName, Patronymic, Position, Pin1);

            MessageBox.Show($"Администратор создан! PIN: {Pin1}", "Успешно");
            ClearAll();

            await _authenticationService.LoginAsync(Pin1);
        }
    }
}