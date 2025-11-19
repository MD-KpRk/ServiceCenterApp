using ServiceCenterApp.Models.Associations;
using System;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class UsedSparePartViewModel : BaseViewModel
    {
        private readonly OrderSparePart _orderSparePart;
        private readonly Action _updateTotalSumCallback;

        public int PartId => _orderSparePart.PartId;

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
                int currentQty = _orderSparePart.Quantity;
                int newQty = value;

                if (newQty < 1) newQty = 1;

                int totalAvailable = currentQty + StockQuantity;

                if (newQty > totalAvailable) newQty = totalAvailable;

                if (currentQty != newQty)
                {
                    // 3. Вычисляем разницу
                    int diff = newQty - currentQty;

                    // 4. Обновляем количество в заказе
                    _orderSparePart.Quantity = newQty;

                    if (_orderSparePart.SparePart != null)
                    {
                        _orderSparePart.SparePart.StockQuantity -= diff;
                    }

                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalSum));
                    OnPropertyChanged(nameof(StockQuantity));

                    _updateTotalSumCallback?.Invoke();
                }
                else if (value != newQty)
                {
                    OnPropertyChanged(nameof(Quantity));
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