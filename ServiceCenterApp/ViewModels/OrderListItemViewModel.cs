using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using System.Linq; // Не забудьте этот using для FirstOrDefault
using System.Windows.Media;

namespace ServiceCenterApp.ViewModels
{
    public class OrderListItemViewModel : BaseViewModel
    {
        public int OrderId { get; set; }

        private string _registrationDate;
        public string RegistrationDate { get => _registrationDate; set { _registrationDate = value; OnPropertyChanged(); } }

        private string _statusName;
        public string StatusName { get => _statusName; set { _statusName = value; OnPropertyChanged(); } }

        private Brush _statusColor;
        public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(); } }

        private string _clientFullName;
        public string ClientFullName { get => _clientFullName; set { _clientFullName = value; OnPropertyChanged(); } }

        private string _deviceDescription;
        public string DeviceDescription { get => _deviceDescription; set { _deviceDescription = value; OnPropertyChanged(); } }

        private string _problemDescription;
        public string ProblemDescription { get => _problemDescription; set { _problemDescription = value; OnPropertyChanged(); } }

        private string _masterFullName;
        public string MasterFullName { get => _masterFullName; set { _masterFullName = value; OnPropertyChanged(); } }

        // Свойство для подсветки (добавлено ранее)
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public OrderListItemViewModel(Order order)
        {
            RefreshData(order);
        }

        public void RefreshData(Order order)
        {
            OrderId = order.OrderId;
            RegistrationDate = order.RegistrationDate.ToString("dd.MM.yyyy");
            StatusName = order.Status?.StatusName ?? "Неизвестен";
            ClientFullName = $"{order.Client?.SurName} {order.Client?.FirstName?.FirstOrDefault()}.";
            DeviceDescription = $"{order.Device?.Brand} {order.Device?.Model}";
            ProblemDescription = order.ProblemDescription;

            if (order.AcceptorEmployee != null)
            {
                MasterFullName = $"{order.AcceptorEmployee.SurName} {order.AcceptorEmployee.FirstName?.FirstOrDefault()}.";
            }
            else
            {
                MasterFullName = "—";
            }

            StatusColor = GetStatusColor((OrderStatusEnum)(order.StatusId));
        }

        private Brush GetStatusColor(OrderStatusEnum status)
        {
            var colorAttribute = status.GetAttribute<StatusColorAttribute>();
            return colorAttribute?.ToBrush() ?? new SolidColorBrush(Colors.SlateGray);
        }
    }
}