using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Associations;
using ServiceCenterApp.Models.Lookup;
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
            }
        }

        // Списки
        private List<OrderStatusViewModel> _allOrderStatuses;
        public List<OrderStatusViewModel> AllOrderStatuses { get => _allOrderStatuses; private set { _allOrderStatuses = value; OnPropertyChanged(); } }
        private List<Employee> _allEmployees;
        public List<Employee> AllEmployees { get => _allEmployees; private set { _allEmployees = value; OnPropertyChanged(); } }
        private List<Priority> _allPriorities;
        public List<Priority> AllPriorities { get => _allPriorities; private set { _allPriorities = value; OnPropertyChanged(); } }

        // Выбранные элементы
        private OrderStatusViewModel _selectedOrderStatus;
        public OrderStatusViewModel SelectedOrderStatus { get => _selectedOrderStatus; set { _selectedOrderStatus = value; OnPropertyChanged(); if (SelectedOrderDetails != null && value != null) SelectedOrderDetails.StatusId = value.StatusId; } }
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
        #endregion

        #region "Клей" между панелями
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
        #endregion

        public OrdersViewModel(ApplicationDbContext context, INavigationService navigationService, IServiceProvider serviceProvider)
        {
            _context = context;
            _navigationService = navigationService;
            _serviceProvider = serviceProvider;

            FilteredOrders = new ObservableCollection<OrderListItemViewModel>();
            StatusFilters = new ObservableCollection<StatusFilterViewModel>();
            UsedSpareParts = new ObservableCollection<UsedSparePartViewModel>();

            SelectOrderCommand = new RelayCommand<OrderListItemViewModel>(vm => SelectedOrderInList = vm);
            AddSparePartCommand = new RelayCommand(ExecuteAddSparePart, CanExecuteAddSparePart);
            SaveChangesCommand = new RelayCommand(ExecuteSaveChanges, CanExecuteSaveChanges);
            CloseDetailsCommand = new RelayCommand(CloseDetails);
            CreateOrderCommand = new RelayCommand(ExecuteCreateOrder);

            InitializeFilters();
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
                CloseDetails();
            }
        }

        private async Task<bool> SaveInternalAsync()
        {
            try
            {
                if (SelectedOrderDetails == null) return false;

                await _context.SaveChangesAsync();

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

                if (SelectedOrderInList != null)
                {
                    SelectedOrderInList.RefreshData(SelectedOrderDetails);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task RefreshAsync()
        {
            if (_context.ChangeTracker.HasChanges())
            {
                _context.ChangeTracker.Clear();
            }

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
                    order.ClientFullName.ToLower().Contains(lowerSearchText) ||
                    order.DeviceDescription.ToLower().Contains(lowerSearchText) ||
                    order.OrderId.ToString().Contains(lowerSearchText)
                );
            }
            FilteredOrders.Clear();
            foreach (var item in result) { FilteredOrders.Add(item); }
        }
    }
}