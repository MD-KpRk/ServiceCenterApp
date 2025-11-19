using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Lookup;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; // Для очистки номера
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media; // Для кистей (Colors)

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
                // Запускаем поиск при каждом изменении
                TryFindClient(value);
            }
        }

        // Статус поиска (для красивого отображения)
        private string _searchStatusText = "Введите номер";
        public string SearchStatusText { get => _searchStatusText; set { _searchStatusText = value; OnPropertyChanged(); } }

        private Brush _searchStatusColor = new SolidColorBrush(Colors.Gray);
        public Brush SearchStatusColor { get => _searchStatusColor; set { _searchStatusColor = value; OnPropertyChanged(); } }

        private string _searchStatusIcon = "?"; // Символ для иконки
        public string SearchStatusIcon { get => _searchStatusIcon; set { _searchStatusIcon = value; OnPropertyChanged(); } }


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

            SelectedPriority = AllPriorities.FirstOrDefault(p => p.PriorityId == (int)PriorityEnum.Normal);
        }

        private async void TryFindClient(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            {
                _existingClient = null;
                SearchStatusText = "Введите номер";
                SearchStatusColor = new SolidColorBrush(Colors.Gray);
                SearchStatusIcon = "...";

                ClearClientFields();
                return;
            }

            // 2. Ищем клиента
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.PhoneNumber.Contains(phoneNumber));

            if (client != null)
            {
                _existingClient = client;

                ClientSurname = client.SurName;
                ClientFirstName = client.FirstName;
                ClientPatronymic = client.Patronymic;
                ClientEmail = client.Email;

                SearchStatusText = "Клиент найден";
                SearchStatusColor = new SolidColorBrush(Colors.Green);
                SearchStatusIcon = "✓";
            }
            else
            {
                _existingClient = null;

                SearchStatusText = "Новый клиент";
                SearchStatusColor = new SolidColorBrush(Colors.DodgerBlue);
                SearchStatusIcon = "+";
                ClearClientFields();
            }
        }

        private void ClearClientFields()
        {
            ClientSurname = string.Empty;
            ClientFirstName = string.Empty;
            ClientPatronymic = string.Empty;
            ClientEmail = string.Empty;
        }

        private async void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(ClientPhoneNumber) || string.IsNullOrWhiteSpace(ClientSurname) || string.IsNullOrWhiteSpace(ClientFirstName))
            {
                MessageBox.Show("Заполните обязательные поля клиента (Телефон, Фамилия, Имя).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(DeviceType) || string.IsNullOrWhiteSpace(DeviceBrand) || string.IsNullOrWhiteSpace(DeviceModel))
            {
                MessageBox.Show("Заполните информацию об устройстве.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(ProblemDescription))
            {
                MessageBox.Show("Укажите описание неисправности.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
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

                var newDevice = new Device
                {
                    DeviceType = DeviceType,
                    Brand = DeviceBrand,
                    Model = DeviceModel,
                    SerialNumber = DeviceSerialNumber
                };
                _context.Devices.Add(newDevice);

                int newStatusId = (int)OrderStatusEnum.New;

                var newOrder = new Order
                {
                    RegistrationDate = DateTime.Now,
                    Client = clientToUse,
                    Device = newDevice,
                    StatusId = newStatusId,
                    PriorityId = SelectedPriority.PriorityId,
                    ProblemDescription = ProblemDescription,
                    Comment = Comment,
                    CreatorEmployeeId = _currentUserService.CurrentUser.EmployeeId,
                    AcceptorEmployeeId = SelectedMaster?.EmployeeId
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

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