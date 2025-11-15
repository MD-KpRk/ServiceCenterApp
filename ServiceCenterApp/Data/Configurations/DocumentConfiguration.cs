using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            // Дата создания по умолчанию - текущая дата
            builder.Property(d => d.CreationDate).HasDefaultValueSql("GETDATE()");

            builder.HasOne(d => d.Order)
                .WithMany(o => o.Documents)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении заказа - удалить документ.

            builder.HasOne(d => d.Employee)
                .WithMany(e => e.AuthoredDocuments)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict); // ЗАПРЕТИТЬ удаление сотрудника, если у него есть документы.
        }
    }
}