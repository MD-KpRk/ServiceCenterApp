using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasIndex(s => s.Name).IsUnique();
            builder.Property(s => s.BasePrice).HasColumnType("decimal(10, 2)");

            builder.HasData(
                new Service { ServiceId = 1, Name = "Диагностика (Первичная)", BasePrice = 0 },
                new Service { ServiceId = 2, Name = "Диагностика (С разборкой)", BasePrice = 30 },
                new Service { ServiceId = 3, Name = "Чистка от пыли + Термопаста", BasePrice = 50 },
                new Service { ServiceId = 4, Name = "Установка Windows + Драйверы", BasePrice = 45 },
                new Service { ServiceId = 5, Name = "Пайка (Сложный ремонт)", BasePrice = 100 }
            );
        }
    }
}