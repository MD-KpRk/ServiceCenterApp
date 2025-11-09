using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Дата регистрации по умолчанию - текущая дата на сервере БД
            builder.Property(o => o.RegistrationDate).HasDefaultValueSql("GETDATE()");

            // Запрет на удаление Клиента, если у него есть Заказы
            builder.HasOne(o => o.Client)
                   .WithMany(c => c.Orders)
                   .HasForeignKey(o => o.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Запрет на удаление Сотрудника, если он принял Заказы
            builder.HasOne(o => o.Employee)
                   .WithMany(e => e.AcceptedOrders)
                   .HasForeignKey(o => o.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}