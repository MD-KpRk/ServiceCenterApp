using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Data.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            // Уникальное название должности
            builder.HasIndex(p => p.PositionName).IsUnique();

        }
    }
}