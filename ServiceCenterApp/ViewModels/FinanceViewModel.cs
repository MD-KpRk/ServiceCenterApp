using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class FinanceViewModel : BaseViewModel, IRefreshable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<FinancialTransaction> Transactions { get; } = new();

        // --- KPI ---
        private decimal _currentBalance;
        public decimal CurrentBalance { get => _currentBalance; set { _currentBalance = value; OnPropertyChanged(); } }

        private decimal _incomeThisMonth;
        public decimal IncomeThisMonth { get => _incomeThisMonth; set { _incomeThisMonth = value; OnPropertyChanged(); } }

        private decimal _expenseThisMonth;
        public decimal ExpenseThisMonth { get => _expenseThisMonth; set { _expenseThisMonth = value; OnPropertyChanged(); } }

        public ICommand AddExpenseCommand { get; }

        public FinanceViewModel(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            AddExpenseCommand = new RelayCommand(ExecuteAddExpense);
        }

        public async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var list = await _context.FinancialTransactions
                .AsNoTracking()
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            Transactions.Clear();
            foreach (var t in list) Transactions.Add(t);

            var totalIncome = list.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpense = list.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            CurrentBalance = totalIncome - totalExpense;

            IncomeThisMonth = list.Where(t => t.Type == TransactionType.Income && t.Date >= startOfMonth).Sum(t => t.Amount);
            ExpenseThisMonth = list.Where(t => t.Type == TransactionType.Expense && t.Date >= startOfMonth).Sum(t => t.Amount);
        }

        private async void ExecuteAddExpense()
        {
            var vm = new AddTransactionViewModel(_context);

            // Инициализируем как РАСХОД
            await vm.InitializeAsync(TransactionType.Expense);

            var window = new AddTransactionWindow(vm);

            if (window.ShowDialog() == true)
            {
                _ = LoadDataAsync();
            }
        }
    }
}