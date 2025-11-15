using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class OrdersPageViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INavigationService _navigationService; 

        // ПОЛНЫЙ список заказов, загруженный из БД
        private List<OrderListItemViewModel> _allOrders = new List<OrderListItemViewModel>();

        // Коллекция, которая отображается в UI
        public ObservableCollection<OrderListItemViewModel> FilteredOrders { get; set; }

        // Коллекция для хранения галочек-фильтров
        public ObservableCollection<StatusFilterViewModel> StatusFilters { get; set; }

        public ICommand OpenOrderDetailsCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters(); 
            }
        }

        public OrdersPageViewModel(ApplicationDbContext context, INavigationService navigationService)
        {
            _context = context;
            _navigationService = navigationService;
            FilteredOrders = new ObservableCollection<OrderListItemViewModel>();
            StatusFilters = new ObservableCollection<StatusFilterViewModel>();

            OpenOrderDetailsCommand = new RelayCommand<int>(OpenOrderDetails);
            InitializeFilters();
        }

        private void OpenOrderDetails(int orderId)
        {
            _navigationService.NavigateTo<OrderInfoPageViewModel>(orderId);
        }

        private void InitializeFilters()
        {
            var statusValues = Enum.GetValues(typeof(OrderStatusEnum)).Cast<OrderStatusEnum>();
            foreach (var status in statusValues)
            {
                StatusFilters.Add(new StatusFilterViewModel(status.GetDescription(), ApplyFilters));
            }
        }

        public async Task LoadOrdersAsync()
        {
            var ordersFromDb = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.Client)
                .Include(o => o.Device)
                .Include(o => o.CreatorEmployee)
                .OrderByDescending(o => o.RegistrationDate)
                .ToListAsync();

            _allOrders = ordersFromDb.Select(order => new OrderListItemViewModel(order)).ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var selectedStatuses = StatusFilters
                .Where(f => f.IsChecked)
                .Select(f => f.StatusName)
                .ToHashSet(); 

            var result = _allOrders
                .Where(order => selectedStatuses.Contains(order.StatusName));

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
            foreach (var item in result)
            {
                FilteredOrders.Add(item);
            }
        }
    }
}