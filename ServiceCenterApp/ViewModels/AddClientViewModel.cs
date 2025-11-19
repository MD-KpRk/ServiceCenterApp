using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces; // Убедись, что этот namespace есть, если там BaseViewModel
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddClientViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;

        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }

        public ICommand SaveCommand { get; }

        public AddClientViewModel(ApplicationDbContext context)
        {
            _context = context;
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        private async void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(Surname) || string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(PhoneNumber))
            {
                MessageBox.Show("Заполните обязательные поля (Фамилия, Имя, Телефон).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newClient = new Client
                {
                    SurName = Surname,
                    FirstName = FirstName,
                    Patronymic = Patronymic,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    Comment = Comment
                };

                _context.Clients.Add(newClient);
                await _context.SaveChangesAsync();

                // Закрываем окно
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
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}