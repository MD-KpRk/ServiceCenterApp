using ServiceCenterApp.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class DocumentSettingsViewModel : BaseViewModel
    {
        public Dictionary<string, int> WarrantyOptions { get; } = new Dictionary<string, int>
        {
            { "Без гарантии", 0 },
            { "14 дней", 14 },
            { "1 месяц (30 дней)", 30 },
            { "3 месяца (90 дней)", 90 },
            { "4 месяца (120 дней)", 120 },
            { "6 месяцев (180 дней)", 180 },
            { "1 год (365 дней)", 365 }
        };

        private int _selectedDays;
        public int SelectedDays
        {
            get => _selectedDays;
            set
            {
                _selectedDays = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ValidUntilText));
            }
        }

        public string ValidUntilText
        {
            get
            {
                if (SelectedDays == 0) return "Гарантия не предоставляется";
                var date = DateTime.Now.AddDays(SelectedDays);
                return $"Действует до: {date:dd.MM.yyyy}";
            }
        }

        public ICommand ConfirmCommand { get; }

        public DocumentSettingsViewModel(int currentWarrantyDays)
        {
            // Если в заказе уже стоит гарантия (например 0), ставим её. Если нет - 30.
            SelectedDays = currentWarrantyDays == 0 ? 30 : currentWarrantyDays;

            ConfirmCommand = new RelayCommand(Confirm);
        }

        private void Confirm()
        {
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
    }
}