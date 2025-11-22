using ServiceCenterApp.Models.Associations;
using System;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class UsedServiceViewModel : BaseViewModel
    {
        private readonly OrderService _orderService;
        private readonly Action _updateTotalSumCallback;
        private readonly Action<UsedServiceViewModel> _removeCallback;

        public int ServiceId => _orderService.ServiceId;
        public string Name => _orderService.Service?.Name ?? "Неизвестно";

        public decimal Price => _orderService.Price;
        public decimal TotalSum => Price;

        public ICommand RemoveCommand { get; }

        public UsedServiceViewModel(OrderService orderService, Action updateTotalSumCallback, Action<UsedServiceViewModel> removeCallback)
        {
            _orderService = orderService;
            _updateTotalSumCallback = updateTotalSumCallback;
            _removeCallback = removeCallback;

            RemoveCommand = new RelayCommand(() => _removeCallback(this));
        }
    }
}