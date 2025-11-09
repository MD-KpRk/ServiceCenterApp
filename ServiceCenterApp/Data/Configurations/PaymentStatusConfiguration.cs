using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Data.Configurations
{
    public class PaymentStatusConfiguration : IEntityTypeConfiguration<PaymentStatus>
    {
        public void Configure(EntityTypeBuilder<PaymentStatus> builder)
        {
            builder.HasIndex(ps => ps.StatusName).IsUnique();

            builder.HasData(
                new PaymentStatus { PaymentStatusId = 1, StatusName = "Ожидает оплаты" },
                new PaymentStatus { PaymentStatusId = 2, StatusName = "Оплачен" },
                new PaymentStatus { PaymentStatusId = 3, StatusName = "Отменён" }
            );
        }
    }
}