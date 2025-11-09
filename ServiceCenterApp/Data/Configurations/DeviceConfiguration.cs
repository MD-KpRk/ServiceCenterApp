using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasIndex(d => d.SerialNumber)
                   .IsUnique()
                   .HasFilter("[SerialNumber] IS NOT NULL");

            builder.Property(d => d.WarrantyStatus).HasDefaultValue(false);
        }
    }
}