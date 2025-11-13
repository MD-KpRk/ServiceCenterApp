using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ServiceCenterApp.ViewModels
{
    public class EmployeePerformanceViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int InProgressOrdersCount { get; set; }
        public int CompletedThisMonthCount { get; set; }
    }

    public class MainAdminPageViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;

        #region Order KPI Properties

        private int _totalOrdersCount;
        public int TotalOrdersCount
        {
            get => _totalOrdersCount;
            set { _totalOrdersCount = value; OnPropertyChanged(); }
        }

        private int _newOrdersCount;
        public int NewOrdersCount
        {
            get => _newOrdersCount;
            set { _newOrdersCount = value; OnPropertyChanged(); }
        }

        private int _inProgressOrdersCount;
        public int InProgressOrdersCount
        {
            get => _inProgressOrdersCount;
            set { _inProgressOrdersCount = value; OnPropertyChanged(); }
        }

        private int _readyForPickupCount;
        public int ReadyForPickupCount
        {
            get => _readyForPickupCount;
            set { _readyForPickupCount = value; OnPropertyChanged(); }
        }

        #endregion

        #region Financial KPI Properties

        private decimal _totalAmountDue;
        public decimal TotalAmountDue
        {
            get => _totalAmountDue;
            set { _totalAmountDue = value; OnPropertyChanged(); }
        }

        private decimal _earnedToday;
        public decimal EarnedToday
        {
            get => _earnedToday;
            set { _earnedToday = value; OnPropertyChanged(); }
        }

        private decimal _earnedThisMonth;
        public decimal EarnedThisMonth
        {
            get => _earnedThisMonth;
            set { _earnedThisMonth = value; OnPropertyChanged(); }
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged(); } }

        #endregion

        #region Employee Performance Properties

        public ObservableCollection<EmployeePerformanceViewModel> EmployeePerformanceData { get; set; }

        #endregion

        public MainAdminPageViewModel(ApplicationDbContext context)
        {
            _context = context;
            EmployeePerformanceData = new ObservableCollection<EmployeePerformanceViewModel>();
        }

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                // --- ИНФОРМАЦИЯ О ЗАКАЗАХ ---
                TotalOrdersCount = await _context.Orders.CountAsync();
                NewOrdersCount = await _context.Orders.CountAsync(o => o.Status.StatusName == "Новая");
                var inProgressStatuses = new[] { "В диагностике", "Ожидает запчасть", "В работе" };
                InProgressOrdersCount = await _context.Orders.CountAsync(o => o.Status != null && inProgressStatuses.Contains(o.Status.StatusName));
                ReadyForPickupCount = await _context.Orders.CountAsync(o => o.Status.StatusName == "Готов к выдаче");

                // --- ИНФОРМАЦИЯ О ДЕНЬГАХ ---
                TotalAmountDue = await _context.Payments
                    .Where(p => p.PaymentStatus.StatusName == "Ожидает оплаты" && p.Order.Status.StatusName == "Готов к выдаче")
                    .SumAsync(p => p.Amount);

                EarnedToday = await _context.Payments
                    .Where(p => p.PaymentStatus.StatusName == "Оплачен" && p.PaymentDate.Date == today)
                    .SumAsync(p => p.Amount);

                EarnedThisMonth = await _context.Payments
                    .Where(p => p.PaymentStatus.StatusName == "Оплачен" && p.PaymentDate >= startOfMonth)
                    .SumAsync(p => p.Amount);

                TotalRevenue = await _context.Payments
                    .Where(p => p.PaymentStatus.StatusName == "Оплачен")
                    .SumAsync(p => p.Amount);

                // --- ИНФОРМАЦИЯ О СОТРУДНИКАХ ---
                EmployeePerformanceData.Clear(); // Очищаем перед загрузкой новых данных
                var employeeData = await _context.Employees
                    .Where(e => e.AcceptedOrders.Any()) // Только сотрудники с заказами
                    .Include(e => e.Position)
                    .Include(e => e.AcceptedOrders)
                        .ThenInclude(o => o.Status)
                    .Select(e => new EmployeePerformanceViewModel
                    {
                        FullName = $"{e.SurName} {e.FirstName}",
                        Position = e.Position.PositionName,
                        InProgressOrdersCount = e.AcceptedOrders.Count(o => o.Status != null && inProgressStatuses.Contains(o.Status.StatusName)),
                        CompletedThisMonthCount = e.AcceptedOrders.Count(o => o.Status.StatusName == "Выдан" && o.EndDate.HasValue && o.EndDate.Value >= startOfMonth)
                    })
                    .OrderByDescending(e => e.InProgressOrdersCount) // Сортируем по занятости
                    .ToListAsync();

                foreach (var emp in employeeData)
                {
                    EmployeePerformanceData.Add(emp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}