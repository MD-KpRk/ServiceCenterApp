using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            // Первичный ключ
            builder.HasKey(x => x.HistoryId);

            // Связь с Заказом
            // Если удаляем Заказ -> История удаляется (это нормально)
            builder.HasOne(h => h.Order)
                   .WithMany()
                   .HasForeignKey(h => h.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Связь с Сотрудником
            // Если удаляем Сотрудника -> Запретить удаление, если есть история (или просто разорвать связь)
            builder.HasOne(h => h.Employee)
                   .WithMany()
                   .HasForeignKey(h => h.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Связь со Старым статусом
            // ON DELETE NO ACTION (Restrict)
            builder.HasOne(h => h.OldStatus)
                   .WithMany()
                   .HasForeignKey(h => h.OldStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Связь с Новым статусом
            // ON DELETE NO ACTION (Restrict)
            builder.HasOne(h => h.NewStatus)
                   .WithMany()
                   .HasForeignKey(h => h.NewStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}