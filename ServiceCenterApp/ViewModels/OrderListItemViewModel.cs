using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using System.Windows.Media;

namespace ServiceCenterApp.ViewModels
{
    public class OrderListItemViewModel : BaseViewModel
    {
        public int OrderId { get; set; }
        public string RegistrationDate { get; set; }
        public string StatusName { get; set; }
        public Brush StatusColor { get; set; }
        public string ClientFullName { get; set; }
        public string DeviceDescription { get; set; }
        public string ProblemDescription { get; set; }
        public string MasterFullName { get; set; }

        public OrderListItemViewModel(Order order)
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