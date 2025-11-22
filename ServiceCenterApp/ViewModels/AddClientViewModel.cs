using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddClientViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        public string Surname { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }

        public ICommand SaveCommand { get; }

        public AddClientViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var newClient = new Client
                    {
                        SurName = Surname,
                        FirstName = FirstName,
                        Patronymic = Patronymic,
                        PhoneNumber = PhoneNumber,
                        Email = Email,
                        Comment = Comment
                    };

                    context.Clients.Add(newClient);
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
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}