using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddTransactionViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
        public int? RelatedOrderId { get; set; }
        public List<TransactionCategory> AvailableCategories { get; private set; }
        public TransactionCategory SelectedCategory { get; set; }

        public PaymentMethod SelectedMethod { get; set; } = PaymentMethod.Cash;
        public Dictionary<string, PaymentMethod> PaymentMethods { get; } = new()
        {
            { "Наличные", PaymentMethod.Cash },
            { "Безнал (Карта/Счет)", PaymentMethod.Card }
        };

        public ICommand SaveCommand { get; }

        public AddTransactionViewModel(ApplicationDbContext context)
        {
            _context = context;
            SaveCommand = new RelayCommand(ExecuteSave);
        }
        public async Task InitializeAsync(TransactionType type)
        {
            TransactionType = type;

            // Грузим категории: Если Расход -> IsExpense=true, Если Приход -> IsExpense=false
            bool isExpense = (type == TransactionType.Expense);

            AvailableCategories = await _context.TransactionCategories
                .AsNoTracking()
                .Where(c => c.IsExpense == isExpense)
                .ToListAsync();

            OnPropertyChanged(nameof(AvailableCategories));

            if (AvailableCategories.Any())
                SelectedCategory = AvailableCategories.First();
        }

        private async void ExecuteSave()
        {
            if (Amount <= 0) { MessageBox.Show("Сумма должна быть больше 0"); return; }
            if (SelectedCategory == null) { MessageBox.Show("Выберите категорию"); return; }
            if (string.IsNullOrWhiteSpace(Description)) { MessageBox.Show("Введите описание"); return; }

            try
            {
                var transaction = new FinancialTransaction
                {
                    Date = DateTime.Now,
                    Amount = Amount,
                    Type = TransactionType,     // Берем из настройки
                    CategoryId = SelectedCategory.CategoryId,
                    PaymentMethod = SelectedMethod,
                    Description = Description,
                    RelatedOrderId = RelatedOrderId // Если есть привязка к заказу
                };

                _context.FinancialTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this) { window.DialogResult = true; window.Close(); break; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}