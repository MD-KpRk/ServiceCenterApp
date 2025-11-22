using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Associations;
using ServiceCenterApp.Models.Lookup;
using ServiceCenterApp.Services; // Для PrintService
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel, IRefreshable
    {
        private readonly ApplicationDbContext _context;
        private readonly INavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUserService _currentUserService; // кто менял
        private readonly IPrintService _printService;

        // --- HISTORY ---
        public ObservableCollection<OrderStatusHistory> StatusHistory { get; } = new();

        #region Свойства для левой панели (Список)
        private List<OrderListItemViewModel> _allOrders = new List<OrderListItemViewModel>();
        public ObservableCollection<OrderListItemViewModel> FilteredOrders { get; }
        public ObservableCollection<StatusFilterViewModel> StatusFilters { get; }
        public ICommand SelectOrderCommand { get; }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }
        #endregion

        #region Свойства для правой панели (Детали)
        private Order _selectedOrderDetails;
        public Order SelectedOrderDetails
        {
            get => _selectedOrderDetails;
            private set
            {
                _selectedOrderDetails = value;
                OnPropertyChanged();
                UpdateUsedSpareParts();
                LoadHistoryAsync(value?.OrderId); // Загружаем историю
            }
        }

        private List<OrderStatusViewModel> _allOrderStatuses;
        public List<OrderStatusViewModel> AllOrderStatuses { get => _allOrderStatuses; private set { _allOrderStatuses = value; OnPropertyChanged(); } }
        private List<Employee> _allEmployees;
        public List<Employee> AllEmployees { get => _allEmployees; private set { _allEmployees = value; OnPropertyChanged(); } }
        private List<Priority> _allPriorities;
        public List<Priority> AllPriorities { get => _allPriorities; private set { _allPriorities = value; OnPropertyChanged(); } }

        private OrderStatusViewModel _selectedOrderStatus;
        public OrderStatusViewModel SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set
            {
                _selectedOrderStatus = value;
                OnPropertyChanged();
                if (SelectedOrderDetails != null && value != null) SelectedOrderDetails.StatusId = value.StatusId;
            }
        }
        private Employee _selectedAcceptorEmployee;
        public Employee SelectedAcceptorEmployee { get => _selectedAcceptorEmployee; set { _selectedAcceptorEmployee = value; OnPropertyChanged(); if (SelectedOrderDetails != null) SelectedOrderDetails.AcceptorEmployeeId = value?.EmployeeId; } }
        private Priority _selectedPriority;
        public Priority SelectedPriority { get => _selectedPriority; set { _selectedPriority = value; OnPropertyChanged(); if (SelectedOrderDetails != null && value != null) SelectedOrderDetails.PriorityId = value.PriorityId; } }

        public ObservableCollection<UsedSparePartViewModel> UsedSpareParts { get; }
        private decimal _sparePartsTotalSum;
        public decimal SparePartsTotalSum { get => _sparePartsTotalSum; set { _sparePartsTotalSum = value; OnPropertyChanged(); } }

        public ICommand SaveChangesCommand { get; }
        public ICommand AddSparePartCommand { get; }
        public ICommand CloseDetailsCommand { get; }
        public ICommand CreateOrderCommand { get; }
        public ICommand PrintReceiptCommand { get; } // Команда печати

        private readonly IDocumentService _documentService; 

        public ObservableCollection<Document> OrderDocuments { get; } = new();

        public ICommand GenerateReceptionActCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        #endregion

        private OrderListItemViewModel _selectedOrderInList;
        public OrderListItemViewModel SelectedOrderInList
        {
            get => _selectedOrderInList;
            set
            {
                if (_selectedOrderInList == value) return;
                if (ShouldCancelSelectionChange(value))
                {
                    OnPropertyChanged(nameof(SelectedOrderInList));
                    return;
                }

                if (_selectedOrderInList != null) _selectedOrderInList.IsSelected = false;
                _selectedOrderInList = value;
                if (_selectedOrderInList != null) _selectedOrderInList.IsSelected = true;

                OnPropertyChanged();
                LoadOrderDetailsAsync(value?.OrderId);
            }
        }

        // Конструктор обновлен (добавлены сервисы)
        public OrdersViewModel(ApplicationDbContext context,
                               INavigationService navigationService,
                               IServiceProvider serviceProvider,
                               ICurrentUserService currentUserService,
                               IPrintService printService,
                               IDocumentService documentService)
        {
            _context = context;
            _navigationService = navigationService;
            _serviceProvider = serviceProvider;
            _currentUserService = currentUserService;
            _printService = printService;

            FilteredOrders = new ObservableCollection<OrderListItemViewModel>();
            StatusFilters = new ObservableCollection<StatusFilterViewModel>();
            UsedSpareParts = new ObservableCollection<UsedSparePartViewModel>();

            SelectOrderCommand = new RelayCommand<OrderListItemViewModel>(vm => SelectedOrderInList = vm);
            AddSparePartCommand = new RelayCommand(ExecuteAddSparePart, CanExecuteAddSparePart);
            SaveChangesCommand = new RelayCommand(ExecuteSaveChanges, CanExecuteSaveChanges);
            CloseDetailsCommand = new RelayCommand(CloseDetails);
            CreateOrderCommand = new RelayCommand(ExecuteCreateOrder);
            PrintReceiptCommand = new RelayCommand(ExecutePrintReceipt, CanExecuteSaveChanges); // Можно печатать если есть заказ

            _documentService = documentService;

            GenerateReceptionActCommand = new RelayCommand(ExecuteGenerateReceptionAct, CanExecuteSaveChanges);
            OpenDocumentCommand = new RelayCommand<string>(ExecuteOpenDocument);

            InitializeFilters();
        }

        private async void LoadDocumentsAsync(int? orderId)
        {
            OrderDocuments.Clear();
            if (orderId == null) return;

            var docs = await _documentService.GetDocumentsByOrderIdAsync(orderId.Value);
            foreach (var doc in docs)
            {
                OrderDocuments.Add(doc);
            }
        }

        private async void ExecuteGenerateReceptionAct()
        {
            if (SelectedOrderDetails == null) return;

            try
            {
                var docFlow = _printService.CreateReceptionDocument(SelectedOrderDetails);

                // 2. Сохраняем на диск и в базу (ID типа документа = 1, см. DocumentTypeConfiguration)
                await _documentService.CreateAndSaveDocumentAsync(SelectedOrderDetails, 1, docFlow);

                LoadDocumentsAsync(SelectedOrderDetails.OrderId);

                // Сразу предлагаем печать
                // _printService.PrintReceptionReceipt(SelectedOrderDetails);

                MessageBox.Show("Акт приема сформирован и сохранен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания документа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenDocument(string filePath)
        {
            try
            {
                _documentService.OpenDocument(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecutePrintReceipt()
        {
            if (SelectedOrderDetails == null) return;
            _printService.PrintReceptionReceipt(SelectedOrderDetails);
        }

        // Загрузка истории
        private async void LoadHistoryAsync(int? orderId)
        {
            StatusHistory.Clear();
            if (orderId == null) return;

            var history = await _context.OrderStatusHistories
                .AsNoTracking()
                .Where(h => h.OrderId == orderId)
                .Include(h => h.OldStatus)
                .Include(h => h.NewStatus)
                .Include(h => h.Employee)
                .OrderByDescending(h => h.ChangeDate)
                .ToListAsync();

            foreach (var item in history)
            {
                StatusHistory.Add(item);
            }
        }

        private async void ExecuteCreateOrder()
        {
            var addOrderWindow = (AddOrderWindow)_serviceProvider.GetService(typeof(AddOrderWindow));
            if (addOrderWindow.ShowDialog() == true)
            {
                await LoadAllOrdersListAsync();
            }
        }

        private bool ShouldCancelSelectionChange(OrderListItemViewModel targetOrder)
        {
            if (!_context.ChangeTracker.HasChanges())
            {
                _context.ChangeTracker.Clear();
                return false;
            }

            var result = MessageBox.Show(
                "У вас есть несохраненные изменения. Сохранить их перед переходом?",
                "Несохраненные изменения",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveAndNavigateAsync(targetOrder);
                    return true;
                case MessageBoxResult.No:
                    _context.ChangeTracker.Clear();
                    return false;
                case MessageBoxResult.Cancel:
                    return true;
            }
            return false;
        }

        private async void SaveAndNavigateAsync(OrderListItemViewModel targetOrder)
        {
            if (await SaveInternalAsync())
            {
                _context.ChangeTracker.Clear();
                SelectedOrderInList = targetOrder;
            }
        }

        private void CloseDetails()
        {
            SelectedOrderInList = null;
        }

        private bool CanExecuteSaveChanges() => SelectedOrderDetails != null;

        private async void ExecuteSaveChanges()
        {
            if (await SaveInternalAsync())
            {
                MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                if (SelectedOrderInList != null)
                {
                    SelectedOrderInList.RefreshData(SelectedOrderDetails);
                }
            }
        }

        private async Task<bool> SaveInternalAsync()
        {
            if (SelectedOrderDetails == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Логика истории изменений статуса
                var originalStatusId = (int?)_context.Entry(SelectedOrderDetails).Property(o => o.StatusId).OriginalValue;
                var newStatusId = SelectedOrderDetails.StatusId;

                if (originalStatusId.HasValue && originalStatusId.Value != newStatusId)
                {
                    var historyRecord = new OrderStatusHistory
                    {
                        OrderId = SelectedOrderDetails.OrderId,
                        OldStatusId = originalStatusId.Value,
                        NewStatusId = newStatusId,
                        EmployeeId = _currentUserService.CurrentUser.EmployeeId,
                        ChangeDate = DateTime.Now
                    };
                    _context.OrderStatusHistories.Add(historyRecord);
                }

                // Стандартное сохранение связей
                if (SelectedOrderDetails.StatusId != 0)
                {
                    SelectedOrderDetails.Status = await _context.OrderStatuses.FindAsync(SelectedOrderDetails.StatusId);
                }

                if (SelectedOrderDetails.AcceptorEmployeeId.HasValue)
                {
                    SelectedOrderDetails.AcceptorEmployee = await _context.Employees.FindAsync(SelectedOrderDetails.AcceptorEmployeeId);
                }
                else
                {
                    SelectedOrderDetails.AcceptorEmployee = null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Обновляем UI истории после сохранения
                if (originalStatusId.HasValue && originalStatusId.Value != newStatusId)
                {
                    LoadHistoryAsync(SelectedOrderDetails.OrderId);
                }

                if (SelectedOrderInList != null)
                {
                    SelectedOrderInList.RefreshData(SelectedOrderDetails);
                }
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task RefreshAsync()
        {
            if (_context.ChangeTracker.HasChanges()) _context.ChangeTracker.Clear();
            SearchText = string.Empty;
            if (_selectedOrderInList != null) _selectedOrderInList.IsSelected = false;
            _selectedOrderInList = null;
            OnPropertyChanged(nameof(SelectedOrderInList));
            LoadOrderDetailsAsync(null);
            await LoadInitialDataAsync();
        }

        private void InitializeFilters()
        {
            var statusValues = Enum.GetValues(typeof(OrderStatusEnum)).Cast<OrderStatusEnum>();
            foreach (var status in statusValues)
            {
                StatusFilters.Add(new StatusFilterViewModel(status.GetDescription(), ApplyFilters));
            }
        }

        public async Task LoadInitialDataAsync()
        {
            await LoadComboBoxSourcesAsync();
            await LoadAllOrdersListAsync();
        }

        private async Task LoadAllOrdersListAsync()
        {
            var ordersFromDb = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Status)
                .Include(o => o.Client)
                .Include(o => o.Device)
                .Include(o => o.AcceptorEmployee)
                .OrderByDescending(o => o.RegistrationDate)
                .ToListAsync();

            _allOrders = ordersFromDb.Select(order => new OrderListItemViewModel(order)).ToList();
            ApplyFilters();
        }

        private async Task LoadComboBoxSourcesAsync()
        {
            var statusesFromDb = await _context.OrderStatuses.ToListAsync();
            AllOrderStatuses = statusesFromDb.Select(s => new OrderStatusViewModel(s)).ToList();
            AllEmployees = await _context.Employees.ToListAsync();
            AllPriorities = await _context.Priorities.ToListAsync();
        }

        private async void LoadOrderDetailsAsync(int? orderId)
        {
            if (orderId == null)
            {
                SelectedOrderDetails = null;
                return;
            }

            SelectedOrderDetails = await _context.Orders
                .Include(o => o.CreatorEmployee)
                .Include(o => o.AcceptorEmployee)
                .Include(o => o.Status)
                .Include(o => o.Priority)
                .Include(o => o.Client)
                .Include(o => o.Device)
                .Include(o => o.OrderSpareParts)
                    .ThenInclude(osp => osp.SparePart)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (SelectedOrderDetails != null)
            {
                SelectedOrderStatus = AllOrderStatuses.FirstOrDefault(s => s.StatusId == SelectedOrderDetails.StatusId);
                SelectedAcceptorEmployee = AllEmployees.FirstOrDefault(e => e.EmployeeId == SelectedOrderDetails.AcceptorEmployeeId);
                SelectedPriority = AllPriorities.FirstOrDefault(p => p.PriorityId == SelectedOrderDetails.PriorityId);
            }
        }

        private void UpdateUsedSpareParts()
        {
            UsedSpareParts.Clear();
            if (SelectedOrderDetails?.OrderSpareParts != null)
            {
                foreach (var part in SelectedOrderDetails.OrderSpareParts)
                {
                    UsedSpareParts.Add(new UsedSparePartViewModel(part, CalculateSparePartsTotalSum));
                }
            }
            CalculateSparePartsTotalSum();
        }

        private void CalculateSparePartsTotalSum()
        {
            SparePartsTotalSum = UsedSpareParts.Sum(p => p.TotalSum);
        }

        private bool CanExecuteAddSparePart() => SelectedOrderDetails != null;

        private async void ExecuteAddSparePart()
        {
            var addSparePartWindow = (AddSparePartWindow)_serviceProvider.GetService(typeof(AddSparePartWindow));
            var existingQuantities = new Dictionary<int, int>();
            foreach (var partVm in UsedSpareParts)
            {
                if (existingQuantities.ContainsKey(partVm.PartId))
                    existingQuantities[partVm.PartId] += partVm.Quantity;
                else
                    existingQuantities.Add(partVm.PartId, partVm.Quantity);
            }

            await addSparePartWindow.ViewModel.LoadSparePartsAsync(existingQuantities);

            if (addSparePartWindow.ShowDialog() == true)
            {
                var selectedPart = addSparePartWindow.ViewModel.SelectedSparePart;
                var existingPartVM = UsedSpareParts.FirstOrDefault(p => p.PartNumber == selectedPart.PartNumber);

                if (existingPartVM != null)
                {
                    existingPartVM.Quantity++;
                }
                else
                {
                    var trackedPart = await _context.SpareParts.FindAsync(selectedPart.PartId);
                    if (trackedPart != null)
                    {
                        trackedPart.StockQuantity -= 1;
                        var newOrderSparePart = new OrderSparePart
                        {
                            Order = SelectedOrderDetails,
                            OrderId = SelectedOrderDetails.OrderId,
                            SparePart = trackedPart,
                            PartId = trackedPart.PartId,
                            Quantity = 1
                        };
                        SelectedOrderDetails.OrderSpareParts.Add(newOrderSparePart);
                        UsedSpareParts.Add(new UsedSparePartViewModel(newOrderSparePart, CalculateSparePartsTotalSum));
                    }
                }
                CalculateSparePartsTotalSum();
            }
        }

        private void ApplyFilters()
        {
            var selectedStatuses = StatusFilters.Where(f => f.IsChecked).Select(f => f.StatusName).ToHashSet();
            var result = _allOrders.Where(order => selectedStatuses.Contains(order.StatusName));
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lowerSearchText = SearchText.ToLower();
                result = result.Where(order =>
                    (order.ClientFullName != null && order.ClientFullName.ToLower().Contains(lowerSearchText)) ||
                    (order.DeviceDescription != null && order.DeviceDescription.ToLower().Contains(lowerSearchText)) ||
                    order.OrderId.ToString().Contains(lowerSearchText)
                );
            }
            FilteredOrders.Clear();
            foreach (var item in result) { FilteredOrders.Add(item); }
        }
    }
}