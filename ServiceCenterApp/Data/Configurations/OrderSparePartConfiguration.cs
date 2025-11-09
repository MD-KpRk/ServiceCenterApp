using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Associations;

namespace ServiceCenterApp.Data.Configurations
{
    public class OrderSparePartConfiguration : IEntityTypeConfiguration<OrderSparePart>
    {
        public void Configure(EntityTypeBuilder<OrderSparePart> builder)
        {
            // Составной первичный ключ
            builder.HasKey(osp => new { osp.OrderId, osp.PartId });

            // Количество по умолчанию
            builder.Property(osp => osp.Quantity).HasDefaultValue(1);

            // Запрет на удаление Запчасти, если она используется в Заказах
            builder.HasOne(osp => osp.SparePart)
                   .WithMany(sp => sp.OrderSpareParts)
                   .HasForeignKey(osp => osp.PartId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}