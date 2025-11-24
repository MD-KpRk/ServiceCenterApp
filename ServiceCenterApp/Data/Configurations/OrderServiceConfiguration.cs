using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Associations;

namespace ServiceCenterApp.Data.Configurations
{
    public class OrderServiceConfiguration : IEntityTypeConfiguration<OrderService>
    {
        public void Configure(EntityTypeBuilder<OrderService> builder)
        {
            builder.HasOne(os => os.Order)
                   .WithMany(o => o.OrderServices)
                   .HasForeignKey(os => os.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(os => os.Service)
                   .WithMany(s => s.OrderServices)
                   .HasForeignKey(os => os.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}