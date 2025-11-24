using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations; // Для OrderStatusEnum
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Models; // Для TransactionType
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

    public class MainAdminPageViewModel : BaseViewModel, IRefreshable
    {
        private readonly ApplicationDbContext _context;

        #region KPI Properties
        private int _totalOrdersCount;
        public int TotalOrdersCount { get => _totalOrdersCount; set { _totalOrdersCount = value; OnPropertyChanged(); } }

        private int _newOrdersCount;
        public int NewOrdersCount { get => _newOrdersCount; set { _newOrdersCount = value; OnPropertyChanged(); } }

        private int _inProgressOrdersCount;
        public int InProgressOrdersCount { get => _inProgressOrdersCount; set { _inProgressOrdersCount = value; OnPropertyChanged(); } }

        private int _readyForPickupCount;
        public int ReadyForPickupCount { get => _readyForPickupCount; set { _readyForPickupCount = value; OnPropertyChanged(); } }

        private decimal _totalAmountDue;
        public decimal TotalAmountDue { get => _totalAmountDue; set { _totalAmountDue = value; OnPropertyChanged(); } }

        private decimal _earnedToday;
        public decimal EarnedToday { get => _earnedToday; set { _earnedToday = value; OnPropertyChanged(); } }

        private decimal _earnedThisMonth;
        public decimal EarnedThisMonth { get => _earnedThisMonth; set { _earnedThisMonth = value; OnPropertyChanged(); } }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set { _totalRevenue = value; OnPropertyChanged(); } }
        #endregion

        public ObservableCollection<EmployeePerformanceViewModel> EmployeePerformanceData { get; set; }

        public MainAdminPageViewModel(ApplicationDbContext context)
        {
            _context = context;
            EmployeePerformanceData = new ObservableCollection<EmployeePerformanceViewModel>();
        }

        public async Task RefreshAsync()
        {
            _context.ChangeTracker.Clear();
            await LoadDashboardDataAsync();
        }

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                // Загружаем заказы для счетчиков
                var orders = await _context.Orders.AsNoTracking().Include(o => o.Status).ToListAsync();

                TotalOrdersCount = orders.Count;
                NewOrdersCount = orders.Count(o => o.StatusId == (int)OrderStatusEnum.New);

                // В работе (4) и другие промежуточные статусы, если добавите
                InProgressOrdersCount = orders.Count(o => o.StatusId == (int)OrderStatusEnum.InProgress);

                // Готовы к выдаче (Если такого статуса нет в Enum, используем логику. 
                // Но у нас Completed = 6 - это уже выдан. 
                // Если "Готов к выдаче" убран, то это поле можно убрать или переделать на Completed).
                // Допустим, ReadyForPickupCount теперь показывает "Выданные".
                ReadyForPickupCount = orders.Count(o => o.StatusId == (int)OrderStatusEnum.Completed);


                // === ФИНАНСЫ (НОВАЯ ЛОГИКА) ===
                // Грузим транзакции с привязкой к заказу
                var transactions = await _context.FinancialTransactions
                    .AsNoTracking()
                    .Include(t => t.RelatedOrder)
                    .Where(t => t.Type == TransactionType.Income) // Только доходы
                    .ToListAsync();

                // Фильтруем: Считаем доход ТОЛЬКО если Заказ завершен (StatusId == 6)
                // ИЛИ если транзакция не привязана к заказу (RelatedOrderId == null)
                var completedTransactions = transactions.Where(t =>
                    t.RelatedOrderId == null ||
                    (t.RelatedOrder != null && t.RelatedOrder.StatusId == (int)OrderStatusEnum.Completed)
                ).ToList();

                // 1. Выручка сегодня (только по закрытым заказам)
                EarnedToday = completedTransactions
                    .Where(t => t.Date.Date == today)
                    .Sum(t => t.Amount);

                // 2. Выручка за месяц
                EarnedThisMonth = completedTransactions
                    .Where(t => t.Date >= startOfMonth)
                    .Sum(t => t.Amount);

                // 3. Общая выручка
                TotalRevenue = completedTransactions.Sum(t => t.Amount);

                // 4. Долги клиентов (Сумма заказов, которые ВЫДАНЫ, но НЕ ОПЛАЧЕНЫ полностью)
                // Это сложнее посчитать точно без перебора всех заказов, пока можно оставить 0 или упростить.
                TotalAmountDue = 0;


                // === KPI СОТРУДНИКОВ ===
                EmployeePerformanceData.Clear();
                var employeeData = await _context.Employees
                    .Where(e => e.AcceptedOrders.Any())
                    .Include(e => e.Position)
                    .Include(e => e.AcceptedOrders)
                    .Select(e => new EmployeePerformanceViewModel
                    {
                        FullName = $"{e.SurName} {e.FirstName}",
                        Position = e.Position.PositionName,
                        // В работе
                        InProgressOrdersCount = e.AcceptedOrders.Count(o => o.StatusId == (int)OrderStatusEnum.InProgress),
                        // Завершено в этом месяце (StatusId == 6)
                        CompletedThisMonthCount = e.AcceptedOrders.Count(o =>
                            o.StatusId == (int)OrderStatusEnum.Completed &&
                            o.EndDate.HasValue &&
                            o.EndDate.Value >= startOfMonth)
                    })
                    .OrderByDescending(e => e.InProgressOrdersCount)
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