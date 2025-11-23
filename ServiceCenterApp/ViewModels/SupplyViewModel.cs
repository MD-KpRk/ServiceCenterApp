using System;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class SupplyViewModel : BaseViewModel
    {
        public int IncomingQuantity { get; set; }
        public decimal IncomingPrice { get; set; }

        public ICommand ConfirmCommand { get; }

        public SupplyViewModel()
        {
            ConfirmCommand = new RelayCommand(Confirm);
        }

        private void Confirm()
        {
            if (IncomingQuantity <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0.");
                return;
            }
            if (IncomingPrice < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной.");
                return;
            }

            // Закрываем окно с результатом True
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