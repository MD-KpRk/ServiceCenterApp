using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models.Lookup;
using System.ComponentModel;

namespace ServiceCenterApp.Data.Configurations
{
    public enum PriorityEnum
    {
        [Description("Низкий")]
        Low = 1,

        [Description("Обычный")]
        Normal = 2,

        [Description("Высокий")]
        High = 3,

        [Description("Наивысший")]
        Highest = 4
    }
    public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
    {
        public void Configure(EntityTypeBuilder<Priority> builder)
        {
            builder.HasIndex(p => p.PriorityName).IsUnique();

            var prioritiesToSeed = Enum.GetValues(typeof(PriorityEnum))
                .Cast<PriorityEnum>()
                .Select(p => new Priority
                {
                    PriorityId = (int)p,
                    PriorityName = p.GetDescription()
                })
                .ToList();

            builder.HasData(prioritiesToSeed);
        }
    }
}