using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Associations;
using ServiceCenterApp.Models.Lookup;
using ServiceCenterApp.Services.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenterApp.ViewModels
{
    public class OrderInfoPageViewModel : BaseViewModel, IViewModelWithParameter
    {
        private readonly ApplicationDbContext _context;
        private readonly INavigationService _navigationService;

        private Order _order;
        public Order Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(); }
        }

        private List<OrderStatusViewModel> _allOrderStatuses;
        public List<OrderStatusViewModel> AllOrderStatuses
        {
            get => _allOrderStatuses;
            private set { _allOrderStatuses = value; OnPropertyChanged(); }
        }

        private List<Employee> _allEmployees;
        public List<Employee> AllEmployees
        {
            get => _allEmployees;
            private set { _allEmployees = value; OnPropertyChanged(); }
        }

        private List<Priority> _allPriorities;
        public List<Priority> AllPriorities
        {
            get => _allPriorities;
            private set { _allPriorities = value; OnPropertyChanged(); }
        }

        private OrderStatusViewModel _selectedOrderStatus;
        public OrderStatusViewModel SelectedOrderStatus
        {
            get => _selectedOrderStatus;
            set
            {
                _selectedOrderStatus = value;
                OnPropertyChanged();
                if (Order != null && value != null)
                {
                    Order.StatusId = value.StatusId;
                }
            }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                if (Order != null && value != null)
                {
                    Order.AcceptorEmployeeId = value.EmployeeId;
                }
            }
        }

        private Priority _selectedPriority;
        public Priority SelectedPriority
        {
            get => _selectedPriority;
            set { _selectedPriority = value; OnPropertyChanged(); if (Order != null && value != null) Order.PriorityId = value.PriorityId; }
        }

        public ObservableCollection<OrderSparePart> UsedSpareParts { get; set; }

        public OrderInfoPageViewModel(ApplicationDbContext context, INavigationService navigationService)
        {
            _context = context;
            _navigationService = navigationService;
            UsedSpareParts = new ObservableCollection<OrderSparePart>();
        }

        public async void SetParameter(object parameter)
        {
            if (parameter is int orderId)
            {
                await LoadStatusesAsync();
                await LoadEmployeesAsync();
                await LoadPrioritiesAsync();
                await LoadOrderDetailsAsync(orderId);
            }
        }

        #region Загрузка данных
        private async Task LoadStatusesAsync()
        {
            var statusesFromDb = await _context.OrderStatuses.ToListAsync();
            AllOrderStatuses = statusesFromDb.Select(s => new OrderStatusViewModel(s)).ToList();
        }
        private async Task LoadEmployeesAsync() => AllEmployees = await _context.Employees.ToListAsync();
        private async Task LoadPrioritiesAsync() => AllPriorities = await _context.Priorities.ToListAsync();

        private async Task LoadOrderDetailsAsync(int orderId)
        {
            Order = await _context.Orders
                .Include(o => o.Status)
                .Include(o => o.CreatorEmployee)
                .Include(o => o.Priority)
                .Include(o => o.Client)
                .Include(o => o.Device)
                .Include(o => o.OrderSpareParts)
                    .ThenInclude(osp => osp.SparePart)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (Order != null)
            {
                SelectedOrderStatus = AllOrderStatuses.FirstOrDefault(s => s.StatusId == Order.StatusId);
                SelectedEmployee = AllEmployees.FirstOrDefault(e => e.EmployeeId == Order.AcceptorEmployeeId);

                SelectedPriority = AllPriorities.FirstOrDefault(p => p.PriorityId == Order.PriorityId);
                if (SelectedPriority == null && AllPriorities.Any())
                {
                    SelectedPriority = AllPriorities.FirstOrDefault(p => p.PriorityName == "Обычный");
                }

                UsedSpareParts.Clear();
                foreach (var part in Order.OrderSpareParts)
                {
                    UsedSpareParts.Add(part);
                }
            }
        }
        #endregion
    }
}