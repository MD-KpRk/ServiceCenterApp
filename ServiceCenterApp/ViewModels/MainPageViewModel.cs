using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ServiceCenterApp.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;

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

        private int _completedTodayCount;
        public int CompletedTodayCount
        {
            get => _completedTodayCount;
            set { _completedTodayCount = value; OnPropertyChanged(); }
        }

        private decimal _totalAmountDue;
        public decimal TotalAmountDue
        {
            get => _totalAmountDue;
            set { _totalAmountDue = value; OnPropertyChanged(); }
        }

        public MainPageViewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                // КОНКРЕТНЫЕ ЗНАЧЕНИЯ ИЗ ВАШЕЙ МИГРАЦИИ
                var today = DateTime.Today;

                // 1. Количество новых заказов (статус "Новая")
                NewOrdersCount = await _context.Orders
                    .CountAsync(o => o.Status.StatusName == "Новая");

                // 2. Количество заказов в работе (несколько статусов)
                var inProgressStatuses = new[] { "В диагностике", "Ожидает запчасть", "В работе" };
                InProgressOrdersCount = await _context.Orders
                    .CountAsync(o => o.Status != null && inProgressStatuses.Contains(o.Status.StatusName));

                // 3. Количество завершенных (выданных) сегодня заказов
                CompletedTodayCount = await _context.Orders
                    .CountAsync(o => o.Status.StatusName == "Выдан" && o.EndDate.HasValue && o.EndDate.Value.Date == today);

                // 4. Общая сумма к оплате
                // Сумма всех платежей со статусом "Ожидает оплаты" для заказов со статусом "Готов к выдаче"
                TotalAmountDue = await _context.Payments
                    .Where(p => p.PaymentStatus.StatusName == "Ожидает оплаты" && p.Order.Status.StatusName == "Готов к выдаче")
                    .SumAsync(p => p.Amount);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}