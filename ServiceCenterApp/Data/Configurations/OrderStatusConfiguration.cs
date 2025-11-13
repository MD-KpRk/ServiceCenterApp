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
        New = 1,

        [Description("В диагностике")]
        InDiagnostics = 2,

        [Description("Ожидает запчасть")]
        AwaitingPart = 3,

        [Description("В работе")]
        InProgress = 4,

        [Description("Готов к выдаче")]
        ReadyForPickup = 5,

        [Description("Выдан")]
        Completed = 6,

        [Description("Отменен")]
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