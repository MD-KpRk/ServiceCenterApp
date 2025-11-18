using ServiceCenterApp.Models.Associations;
using System;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class UsedSparePartViewModel : BaseViewModel
    {
        private readonly OrderSparePart _orderSparePart;
        private readonly Action _updateTotalSumCallback;

        public string Name => _orderSparePart.SparePart?.Name ?? "Неизвестно";
        public string PartNumber => _orderSparePart.SparePart?.PartNumber ?? "—";
        public decimal Price => _orderSparePart.SparePart?.Price ?? 0;
        public int StockQuantity => _orderSparePart.SparePart?.StockQuantity ?? 0;

        public decimal TotalSum => Quantity * Price;

        public int Quantity
        {
            get => _orderSparePart.Quantity;
            set
            {
                int newQuantity = value;

                if (newQuantity < 1) newQuantity = 1;

                // Проверка: не больше остатка на складе
                if (newQuantity > StockQuantity) newQuantity = StockQuantity;

                if (_orderSparePart.Quantity != newQuantity)
                {
                    _orderSparePart.Quantity = newQuantity;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalSum)); // Уведомляем об изменении суммы для этой строки
                    _updateTotalSumCallback?.Invoke(); // Вызываем callback для пересчета ОБЩЕЙ суммы
                }
            }
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        public UsedSparePartViewModel(OrderSparePart orderSparePart, Action updateTotalSumCallback)
        {
            _orderSparePart = orderSparePart ?? throw new ArgumentNullException(nameof(orderSparePart));
            _updateTotalSumCallback = updateTotalSumCallback;

            IncreaseQuantityCommand = new RelayCommand(() => Quantity++);
            DecreaseQuantityCommand = new RelayCommand(() => Quantity--);
        }
    }
}