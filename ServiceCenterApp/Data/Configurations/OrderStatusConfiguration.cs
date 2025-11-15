using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models.Lookup;
using System.ComponentModel;

namespace ServiceCenterApp.Data.Configurations
{
    public enum OrderStatusEnum
    {
        [Description("Новая")]
        [StatusColor("DodgerBlue")]
        New = 1,

        [Description("В диагностике")]
        [StatusColor("Orange")]
        InDiagnostics = 2,

        [Description("Ожидает запчасть")]
        [StatusColor("Gold")]
        AwaitingPart = 3,

        [Description("В работе")]
        [StatusColor("Orange")]
        InProgress = 4,

        [Description("Готов к выдаче")]
        [StatusColor("LimeGreen")]
        ReadyForPickup = 5,

        [Description("Выдан")]
        [StatusColor("Gray")]
        Completed = 6,

        [Description("Отменен")]
        [StatusColor("IndianRed")]
        Cancelled = 7
    }

    public class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
    {
        public void Configure(EntityTypeBuilder<OrderStatus> builder)
        {
            builder.HasIndex(s => s.StatusName).IsUnique();

            var statusesToSeed = Enum.GetValues(typeof(OrderStatusEnum))
                .Cast<OrderStatusEnum>()
                .Select(s => new OrderStatus
                {
                    StatusId = (int)s,
                    StatusName = s.GetDescription()
                })
                .ToList();

            builder.HasData(statusesToSeed);
        }
    }
}