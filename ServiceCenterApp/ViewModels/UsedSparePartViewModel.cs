using ServiceCenterApp.Models.Associations;
using System;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class UsedSparePartViewModel : BaseViewModel
    {
        private readonly OrderSparePart _orderSparePart;
        private readonly Action _updateTotalSumCallback;
        private readonly Action<UsedSparePartViewModel> _removeCallback; 

        public int PartId => _orderSparePart.PartId;

        public string Name => _orderSparePart.SparePart?.Name ?? "Неизвестно";
        public string PartNumber => _orderSparePart.SparePart?.PartNumber ?? "—";
        public decimal Price => _orderSparePart.SalePrice;

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

                if (currentQty != newQty)
                {
                    int diff = newQty - currentQty;

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
            }
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveCommand { get; } 

        public UsedSparePartViewModel(OrderSparePart orderSparePart, Action updateTotalSumCallback, Action<UsedSparePartViewModel> removeCallback)
        {
            _orderSparePart = orderSparePart ?? throw new ArgumentNullException(nameof(orderSparePart));
            _updateTotalSumCallback = updateTotalSumCallback;
            _removeCallback = removeCallback;

            IncreaseQuantityCommand = new RelayCommand(() => Quantity++);
            DecreaseQuantityCommand = new RelayCommand(() => Quantity--);
            RemoveCommand = new RelayCommand(() => _removeCallback(this));
        }
    }
}