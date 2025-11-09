using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasIndex(c => c.PhoneNumber).IsUnique();

            builder.HasIndex(c => c.Email)
                   .IsUnique()
                   .HasFilter("[Email] IS NOT NULL");
        }
    }
}