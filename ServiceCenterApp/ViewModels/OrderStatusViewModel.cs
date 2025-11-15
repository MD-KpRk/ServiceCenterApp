using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models.Lookup;
using System.Windows.Media;

namespace ServiceCenterApp.ViewModels
{
    public class OrderStatusViewModel
    {
        public int StatusId { get; }
        public string StatusName { get; }
        public Brush StatusColor { get; }

        public OrderStatusViewModel(OrderStatus orderStatus)
        {
            StatusId = orderStatus.StatusId;
            StatusName = orderStatus.StatusName;

            var statusEnum = (OrderStatusEnum)orderStatus.StatusId;

            var colorAttribute = statusEnum.GetAttribute<StatusColorAttribute>();
            StatusColor = colorAttribute?.ToBrush() ?? new SolidColorBrush(Colors.SlateGray);
        }
    }
}