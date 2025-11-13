using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models.Lookup;
using System.ComponentModel;

namespace ServiceCenterApp.Data.Configurations
{
    public enum PaymentStatusEnum
    {
        [Description("Ожидает оплаты")]
        AwaitingPayment = 1,

        [Description("Оплачен")]
        Paid = 2,

        [Description("Отменён")]
        Cancelled = 3
    }

    public class PaymentStatusConfiguration : IEntityTypeConfiguration<PaymentStatus>
    {
        public void Configure(EntityTypeBuilder<PaymentStatus> builder)
        {
            builder.HasIndex(ps => ps.StatusName).IsUnique();

            var statusesToSeed = Enum.GetValues(typeof(PaymentStatusEnum))
                .Cast<PaymentStatusEnum>()
                .Select(s => new PaymentStatus
                {
                    PaymentStatusId = (int)s,
                    StatusName = s.GetDescription() 
                })
                .ToList();

            builder.HasData(statusesToSeed);
        }
    }
}