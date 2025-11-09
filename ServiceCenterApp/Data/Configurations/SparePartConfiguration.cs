using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class SparePartConfiguration : IEntityTypeConfiguration<SparePart>
    {
        public void Configure(EntityTypeBuilder<SparePart> builder)
        {
            builder.HasIndex(sp => sp.PartNumber).IsUnique();

            builder.Property(sp => sp.Price).HasColumnType("decimal(10, 2)");

            builder.Property(sp => sp.StockQuantity).HasDefaultValue(0);
        }
    }
}