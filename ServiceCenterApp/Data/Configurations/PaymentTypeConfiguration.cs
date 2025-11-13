using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models.Lookup;
using System.ComponentModel;


namespace ServiceCenterApp.Data.Configurations
{
    public enum PaymentTypeEnum
    {
        [Description("Наличный")]
        Cash = 1,

        [Description("Безналичный")]
        Cashless = 2
    }

    public class PaymentTypeConfiguration : IEntityTypeConfiguration<PaymentType>
    {
        public void Configure(EntityTypeBuilder<PaymentType> builder)
        {
            builder.HasIndex(pt => pt.TypeName).IsUnique();

            var typesToSeed = Enum.GetValues(typeof(PaymentTypeEnum))
                .Cast<PaymentTypeEnum>()
                .Select(t => new PaymentType
                {
                    PaymentTypeId = (int)t,
                    TypeName = t.GetDescription() 
                })
                .ToList();

            builder.HasData(typesToSeed);
        }
    }
}