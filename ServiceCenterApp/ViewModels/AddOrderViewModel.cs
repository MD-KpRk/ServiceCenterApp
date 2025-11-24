using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Lookup;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ServiceCenterApp.ViewModels
{
    public class AddOrderViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        private readonly IPrintService _printService;
        private readonly IDocumentService _documentService;

        // Для Debounce (задержки поиска)
        private CancellationTokenSource? _searchCancellationTokenSource;

        // --- ДАННЫЕ КЛИЕНТА ---
        private string _clientPhoneNumber;
        public string ClientPhoneNumber
        {
            get => _clientPhoneNumber;
            set
            {
                if (_clientPhoneNumber != value)
                {
                    _clientPhoneNumber = value;
                    OnPropertyChanged();
                    // Запускаем поиск с задержкой
                    InitiateClientSearch(value);
                }
            }
        }

        // Статус поиска
        private string _searchStatusText = "Введите номер";
        public string SearchStatusText { get => _searchStatusText; set { _searchStatusText = value; OnPropertyChanged(); } }

        private Brush _searchStatusColor = new SolidColorBrush(Colors.Gray);
        public Brush SearchStatusColor { get => _searchStatusColor; set { _searchStatusColor = value; OnPropertyChanged(); } }

        private string _searchStatusIcon = "?";
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

        // Списки
        public List<Employee> AllEmployees { get; private set; }
        public List<Priority> AllPriorities { get; private set; }

        private Employee _selectedMaster;
        public Employee SelectedMaster { get => _selectedMaster; set { _selectedMaster = value; OnPropertyChanged(); } }

        private Priority _selectedPriority;
        public Priority SelectedPriority { get => _selectedPriority; set { _selectedPriority = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }

        private Client _existingClient;

        public AddOrderViewModel(
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            IPrintService printService,       
            IDocumentService documentService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _printService = printService;
            _documentService = documentService;

            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public async Task LoadDataAsync()
        {
            AllEmployees = await _context.Employees.ToListAsync();
            OnPropertyChanged(nameof(AllEmployees));

            AllPriorities = await _context.Priorities.ToListAsync();
            OnPropertyChanged(nameof(AllPriorities));

            if (AllPriorities.Any())
                SelectedPriority = AllPriorities.FirstOrDefault(p => p.PriorityId == (int)PriorityEnum.Normal);
        }

        private void InitiateClientSearch(string phoneNumber)
        {
            // Отменяем предыдущий поиск
            if (_searchCancellationTokenSource != null)
            {
                _searchCancellationTokenSource.Cancel();
                _searchCancellationTokenSource.Dispose();
            }

            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500);

                    if (token.IsCancellationRequested) return;

                    Application.Current.Dispatcher.Invoke(() => TryFindClient(phoneNumber, token));
                }
                catch (Exception)
                {
                }
            });
        }

        private async void TryFindClient(string phoneNumber, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            {
                _existingClient = null;
                SearchStatusText = "Введите номер";
                SearchStatusColor = new SolidColorBrush(Colors.Gray);
                SearchStatusIcon = "...";
                return;
            }

            SearchStatusText = "Поиск...";
            SearchStatusColor = new SolidColorBrush(Colors.Gray);

            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.PhoneNumber.Contains(phoneNumber), token);

                if (token.IsCancellationRequested) return;

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
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                SearchStatusText = "Ошибка поиска";
                SearchStatusColor = new SolidColorBrush(Colors.Red);
                SearchStatusIcon = "!";
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

            using var transaction = await _context.Database.BeginTransactionAsync();

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

                await _context.SaveChangesAsync();

                var newDevice = new Device
                {
                    DeviceType = DeviceType,
                    Brand = DeviceBrand,
                    Model = DeviceModel,
                    SerialNumber = DeviceSerialNumber
                };
                _context.Devices.Add(newDevice);
                await _context.SaveChangesAsync();

                int statusNewId = (int)OrderStatusEnum.New;
                int statusInProgressId = (int)OrderStatusEnum.InProgress;
                int currentEmployeeId = _currentUserService.CurrentUser.EmployeeId;
                DateTime now = DateTime.Now;

                var newOrder = new Order
                {
                    RegistrationDate = now,
                    ClientId = clientToUse.ClientId,
                    DeviceId = newDevice.DeviceId,
                    StatusId = statusNewId, 
                    PriorityId = SelectedPriority.PriorityId,
                    ProblemDescription = ProblemDescription,
                    Comment = Comment,
                    CreatorEmployeeId = currentEmployeeId,
                    AcceptorEmployeeId = null
                };

                var historyNew = new OrderStatusHistory
                {
                    Order = newOrder, 
                    OldStatusId = statusNewId, 
                    NewStatusId = statusNewId,
                    EmployeeId = currentEmployeeId,
                    ChangeDate = now
                };
                _context.OrderStatusHistories.Add(historyNew);

                if (SelectedMaster != null)
                {
                    newOrder.AcceptorEmployeeId = SelectedMaster.EmployeeId;
                    newOrder.StatusId = statusInProgressId;
                    newOrder.StartDate = now; 

                    var historyProgress = new OrderStatusHistory
                    {
                        Order = newOrder,
                        OldStatusId = statusNewId,        
                        NewStatusId = statusInProgressId, 
                        EmployeeId = currentEmployeeId,
                        ChangeDate = now.AddSeconds(1)
                    };
                    _context.OrderStatusHistories.Add(historyProgress);
                }

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                try
                {
                    FlowDocument docFlow = _printService.CreateReceptionDocument(newOrder);
                    await _documentService.CreateAndSaveDocumentAsync(newOrder, 1, docFlow);
                }
                catch (Exception docEx)
                {
                    MessageBox.Show($"Заказ создан, но ошибка при генерации акта: {docEx.Message}");
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
                await transaction.RollbackAsync();
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}