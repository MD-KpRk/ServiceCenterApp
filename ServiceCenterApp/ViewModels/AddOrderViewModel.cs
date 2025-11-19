using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Lookup;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddOrderViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        // --- ДАННЫЕ КЛИЕНТА ---
        private string _clientPhoneNumber;
        public string ClientPhoneNumber
        {
            get => _clientPhoneNumber;
            set
            {
                _clientPhoneNumber = value;
                OnPropertyChanged();
                // При изменении номера пробуем найти клиента
                TryFindClient(value);
            }
        }

        private string _clientSurname;
        public string ClientSurname { get => _clientSurname; set { _clientSurname = value; OnPropertyChanged(); } }

        private string _clientFirstName;
        public string ClientFirstName { get => _clientFirstName; set { _clientFirstName = value; OnPropertyChanged(); } }

        private string _clientPatronymic;
        public string ClientPatronymic { get => _clientPatronymic; set { _clientPatronymic = value; OnPropertyChanged(); } }

        private string _clientEmail;
        public string ClientEmail { get => _clientEmail; set { _clientEmail = value; OnPropertyChanged(); } }

        // --- ДАННЫЕ УСТРОЙСТВА ---
        public string DeviceType { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceSerialNumber { get; set; }

        // --- ДАННЫЕ ЗАКАЗА ---
        public string ProblemDescription { get; set; }
        public string Comment { get; set; }

        // Списки для выбора
        public List<Employee> AllEmployees { get; private set; }
        public List<Priority> AllPriorities { get; private set; }

        private Employee _selectedMaster;
        public Employee SelectedMaster { get => _selectedMaster; set { _selectedMaster = value; OnPropertyChanged(); } }

        private Priority _selectedPriority;
        public Priority SelectedPriority { get => _selectedPriority; set { _selectedPriority = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }

        // Флаг, что мы нашли существующего клиента (чтобы не создавать дубль)
        private Client _existingClient;

        public AddOrderViewModel(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;

            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public async Task LoadDataAsync()
        {
            AllEmployees = await _context.Employees.ToListAsync();
            OnPropertyChanged(nameof(AllEmployees));

            AllPriorities = await _context.Priorities.ToListAsync();
            OnPropertyChanged(nameof(AllPriorities));

            // По умолчанию ставим приоритет "Обычный"
            SelectedPriority = AllPriorities.FirstOrDefault(p => p.PriorityName == "Обычный");
        }

        private async void TryFindClient(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 3)
            {
                _existingClient = null;
                return;
            }

            // Ищем клиента по точному совпадению телефона
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (client != null)
            {
                _existingClient = client;
                // Автозаполняем поля
                ClientSurname = client.SurName;
                ClientFirstName = client.FirstName;
                ClientPatronymic = client.Patronymic;
                ClientEmail = client.Email;
            }
            else
            {
                // Если клиента не нашли, сбрасываем флаг, но поля не очищаем (вдруг пользователь вводит нового)
                _existingClient = null;
            }
        }

        private async void ExecuteSave()
        {
            // 1. Валидация
            if (string.IsNullOrWhiteSpace(ClientPhoneNumber) || string.IsNullOrWhiteSpace(ClientSurname) || string.IsNullOrWhiteSpace(ClientFirstName))
            {
                MessageBox.Show("Заполните обязательные поля клиента (Телефон, Фамилия, Имя).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(DeviceType) || string.IsNullOrWhiteSpace(DeviceBrand) || string.IsNullOrWhiteSpace(DeviceModel))
            {
                MessageBox.Show("Заполните информацию об устройстве (Тип, Бренд, Модель).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(ProblemDescription))
            {
                MessageBox.Show("Укажите описание неисправности.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Работа с КЛИЕНТОМ
                Client clientToUse = _existingClient;

                if (clientToUse == null)
                {
                    clientToUse = new Client
                    {
                        PhoneNumber = ClientPhoneNumber,
                        FirstName = ClientFirstName,
                        SurName = ClientSurname,
                        Patronymic = ClientPatronymic,
                        Email = ClientEmail
                    };
                    _context.Clients.Add(clientToUse);
                }
                else
                {
                    clientToUse.FirstName = ClientFirstName;
                    clientToUse.SurName = ClientSurname;
                    clientToUse.Patronymic = ClientPatronymic;
                    clientToUse.Email = ClientEmail;
                    _context.Clients.Update(clientToUse);
                }

                // 3. Работа с УСТРОЙСТВОМ
                var newDevice = new Device
                {
                    DeviceType = DeviceType,
                    Brand = DeviceBrand,
                    Model = DeviceModel,
                    SerialNumber = DeviceSerialNumber
                };
                _context.Devices.Add(newDevice);

                // 4. Получаем статус "Новая"
                var newStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Новая");
                if (newStatus == null) throw new Exception("Статус 'Новая' не найден в БД.");

                // 5. Создаем ЗАКАЗ
                var newOrder = new Order
                {
                    RegistrationDate = DateTime.Now,
                    Client = clientToUse,
                    Device = newDevice,
                    StatusId = newStatus.StatusId,
                    PriorityId = SelectedPriority.PriorityId,
                    ProblemDescription = ProblemDescription,
                    Comment = Comment,
                    // Важно: Кто создал заказ
                    CreatorEmployeeId = _currentUserService.CurrentUser.EmployeeId,
                    // Мастера можно назначить сразу или оставить пустым
                    AcceptorEmployeeId = SelectedMaster?.EmployeeId
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // Закрываем окно (через View) с результатом true
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
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}