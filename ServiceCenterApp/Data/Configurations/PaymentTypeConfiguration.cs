using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Lookup;

public class PaymentTypeConfiguration : IEntityTypeConfiguration<PaymentType>
{
    public void Configure(EntityTypeBuilder<PaymentType> builder)
    {
        // Устанавливаем, что название типа платежа должно быть уникальным
        builder.HasIndex(pt => pt.TypeName).IsUnique();

        // Заполняем таблицу начальными данными
        builder.HasData(
            new PaymentType { PaymentTypeId = 1, TypeName = "Наличный" },
            new PaymentType { PaymentTypeId = 2, TypeName = "Безналичный" }
        );
    }
}