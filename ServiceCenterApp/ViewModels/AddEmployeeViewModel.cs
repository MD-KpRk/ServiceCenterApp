using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; 
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddEmployeeViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPasswordHasher _passwordHasher;

        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string Patronymic { get; set; }
        public string ContactInfo { get; set; }
        public string PinCode { get; set; }

        public List<Role> AllRoles { get; private set; }
        public List<Position> AllPositions { get; private set; }

        private Role _selectedRole;
        public Role SelectedRole { get => _selectedRole; set { _selectedRole = value; OnPropertyChanged(); } }

        private Position _selectedPosition;
        public Position SelectedPosition { get => _selectedPosition; set { _selectedPosition = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }

        public AddEmployeeViewModel(IServiceProvider serviceProvider, IPasswordHasher passwordHasher)
        {
            _serviceProvider = serviceProvider;
            _passwordHasher = passwordHasher;
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public async Task LoadDataAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                AllRoles = await context.Roles.AsNoTracking().ToListAsync();
                OnPropertyChanged(nameof(AllRoles));

                AllPositions = await context.Positions.AsNoTracking().ToListAsync();
                OnPropertyChanged(nameof(AllPositions));
            }

            if (AllRoles != null && AllRoles.Any()) SelectedRole = AllRoles.FirstOrDefault();
            if (AllPositions != null && AllPositions.Any()) SelectedPosition = AllPositions.FirstOrDefault();
        }

        private async void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(SurName))
            {
                MessageBox.Show("Введите Имя и Фамилию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(PinCode) || PinCode.Length < 4)
            {
                MessageBox.Show("PIN-код должен содержать минимум 4 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedRole == null || SelectedPosition == null)
            {
                MessageBox.Show("Выберите Роль и Должность.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var newEmployee = new Employee
                    {
                        FirstName = FirstName,
                        SurName = SurName,
                        Patronymic = Patronymic,
                        ContactInfo = ContactInfo,
                        RoleId = SelectedRole.RoleId,
                        PositionId = SelectedPosition.PositionId,
                        PINHash = _passwordHasher.Hash(PinCode)
                    };

                    context.Employees.Add(newEmployee);
                    await context.SaveChangesAsync();
                }

                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.DialogResult = true;
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}