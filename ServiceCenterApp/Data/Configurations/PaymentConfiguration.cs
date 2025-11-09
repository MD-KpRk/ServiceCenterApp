using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            // Дата платежа по умолчанию - текущая дата
            builder.Property(p => p.PaymentDate).HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.Amount).HasColumnType("decimal(10, 2)");
        }
    }
}