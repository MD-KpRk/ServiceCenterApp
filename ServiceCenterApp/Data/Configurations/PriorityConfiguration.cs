using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Lookup;

public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
{
    public void Configure(EntityTypeBuilder<Priority> builder)
    {
        builder.HasIndex(p => p.PriorityName).IsUnique();
        builder.HasData(
            new Priority { PriorityId = 1, PriorityName = "Низкий" },
            new Priority { PriorityId = 2, PriorityName = "Обычный" },
            new Priority { PriorityId = 3, PriorityName = "Высокий" },
            new Priority { PriorityId = 4, PriorityName = "Наивысший" }
        );
    }
}