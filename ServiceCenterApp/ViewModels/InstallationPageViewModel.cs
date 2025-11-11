using ServiceCenterApp.Models; // Предполага се, че този using е нужен
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

        // Свойствата за максимална дължина (остават същите)
        public int MaxFirstNameLength { get; }
        public int MaxSurNameLength { get; }
        public int MaxPatronymicLength { get; }
        public int MaxPositionLength { get; }

        public InstallationPageViewModel()
        {
            // Получаем максимальную длину для свойств класса Employee
            MaxFirstNameLength = GetMaxLength(typeof(Employee), nameof(Employee.FirstName));
            MaxSurNameLength = GetMaxLength(typeof(Employee), nameof(Employee.SurName));
            MaxPatronymicLength = GetMaxLength(typeof(Employee), nameof(Employee.Patronymic));

            // Получаем максимальную длину для свойства из связанного класса Position
            MaxPositionLength = GetMaxLength(typeof(Position), nameof(Models.Position.PositionName));

            CreateAdminCommand = new RelayCommand(CreateAdministrator, CanCreateAdministrator);
        }

        private int GetMaxLength(Type type, string propertyName)
        {
            PropertyInfo? propInfo = type.GetProperty(propertyName);
            if (propInfo != null)
            {
                var maxLengthAttr = propInfo.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLengthAttr != null)
                {
                    return maxLengthAttr.Length;
                }
            }
            return int.MaxValue; 
        }

        private bool CanCreateAdministrator(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(SurName) &&
                   !string.IsNullOrWhiteSpace(Position) &&
                   !string.IsNullOrWhiteSpace(Pin1) &&
                   Pin1 == Pin2; // Проверката вече използва правилните свойства
        }

        private void CreateAdministrator(object? parameter)
        {
            MessageBox.Show($"Администратор создан! PIN: {Pin1}", "Успешно");
        }
    }
}