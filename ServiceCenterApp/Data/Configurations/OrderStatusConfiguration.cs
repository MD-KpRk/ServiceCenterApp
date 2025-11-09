using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Data.Configurations
{
    public class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
    {
        public void Configure(EntityTypeBuilder<OrderStatus> builder)
        {
            builder.HasIndex(s => s.StatusName).IsUnique();
            builder.HasData(
                new OrderStatus { StatusId = 1, StatusName = "Новая" },
                new OrderStatus { StatusId = 2, StatusName = "В диагностике" },
                new OrderStatus { StatusId = 3, StatusName = "Ожидает запчасть" },
                new OrderStatus { StatusId = 4, StatusName = "В работе" },
                new OrderStatus { StatusId = 5, StatusName = "Готов к выдаче" },
                new OrderStatus { StatusId = 6, StatusName = "Выдан" },
                new OrderStatus { StatusId = 7, StatusName = "Отменен" }
            );
        }
    }
}