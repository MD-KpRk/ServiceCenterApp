using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Helpers;
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
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string Patronymic { get; set; }
        public string ContactInfo { get; set; }
        public string PinCode { get; set; } 

        public List<Role> AllRoles { get; private set; }
        public List<Position> AllPositions { get; private set; }

        // Выбранные значения
        private Role _selectedRole;
        public Role SelectedRole { get => _selectedRole; set { _selectedRole = value; OnPropertyChanged(); } }

        private Position _selectedPosition;
        public Position SelectedPosition { get => _selectedPosition; set { _selectedPosition = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }

        public AddEmployeeViewModel(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public async Task LoadDataAsync()
        {
            AllRoles = await _context.Roles.ToListAsync();
            OnPropertyChanged(nameof(AllRoles));

            AllPositions = await _context.Positions.ToListAsync();
            OnPropertyChanged(nameof(AllPositions));

            if (AllRoles.Any()) SelectedRole = AllRoles.FirstOrDefault();
            if (AllPositions.Any()) SelectedPosition = AllPositions.FirstOrDefault();
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
                var newEmployee = new Employee
                {
                    FirstName = FirstName,
                    SurName = SurName,
                    Patronymic = Patronymic,
                    ContactInfo = ContactInfo,
                    RoleId = SelectedRole.RoleId,
                    PositionId = SelectedPosition.PositionId,
                    // ХЭШИРУЕМ ПАРОЛЬ!
                    PINHash = _passwordHasher.Hash(PinCode)
                };

                _context.Employees.Add(newEmployee);
                await _context.SaveChangesAsync();

                // Закрытие окна
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this) { window.DialogResult = true; window.Close(); break; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}