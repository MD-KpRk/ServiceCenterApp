using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPasswordHasher _passwordHasher;

        // Данные для отображения
        public string FullName { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }
        public string ContactInfo { get; set; }

        // Поля для смены PIN
        public string OldPin { get; set; }
        public string NewPin { get; set; }

        public ICommand ChangePinCommand { get; }

        public ProfileViewModel(ICurrentUserService currentUserService, IServiceProvider serviceProvider, IPasswordHasher passwordHasher)
        {
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
            _passwordHasher = passwordHasher;

            ChangePinCommand = new RelayCommand(ExecuteChangePin);

            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = _currentUserService.CurrentUser;
            if (user != null)
            {
                FullName = $"{user.SurName} {user.FirstName} {user.Patronymic}";
                Position = user.Position?.PositionName ?? "—";
                Role = user.Role?.RoleName ?? "—";
                ContactInfo = user.ContactInfo ?? "—";
            }
        }

        private async void ExecuteChangePin()
        {
            if (string.IsNullOrWhiteSpace(OldPin) || string.IsNullOrWhiteSpace(NewPin))
            {
                MessageBox.Show("Введите старый и новый PIN-код.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPin.Length < 4)
            {
                MessageBox.Show("Новый PIN должен быть не короче 4 цифр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUser = _currentUserService.CurrentUser;
            if (currentUser == null) return;

            // Проверяем старый пароль (хэш)
            if (!_passwordHasher.Verify(OldPin, currentUser.PINHash))
            {
                MessageBox.Show("Старый PIN-код введен неверно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Загружаем пользователя из базы для обновления
                    var userDb = await context.Employees.FindAsync(currentUser.EmployeeId);
                    if (userDb != null)
                    {
                        userDb.PINHash = _passwordHasher.Hash(NewPin);
                        await context.SaveChangesAsync();

                        // Обновляем хеш в текущей сессии тоже, чтобы не пришлось перезаходить
                        currentUser.PINHash = userDb.PINHash;

                        MessageBox.Show("PIN-код успешно изменен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Очищаем поля
                        OldPin = "";
                        NewPin = "";
                        OnPropertyChanged(nameof(OldPin));
                        OnPropertyChanged(nameof(NewPin));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене PIN: {ex.Message}");
            }
        }
    }
}