using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models;
using System.ComponentModel;

namespace ServiceCenterApp.Data.Configurations
{
    public enum RoleEnum
    {
        [Description("Администратор")]
        [AdditionalDescription("Полный доступ")]
        Administrator = 1,

        [Description("Приемщик")]
        [AdditionalDescription("Регистрация и выдача заказов")]
        Receptionist = 2,

        [Description("Мастер")]
        [AdditionalDescription("Диагностика и ремонт")]
        Technician = 3
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasIndex(r => r.RoleName).IsUnique();

            var rolesToSeed = Enum.GetValues(typeof(RoleEnum))
                .Cast<RoleEnum>()
                .Select(r => new Role
                {
                    RoleId = (int)r,
                    RoleName = r.GetDescription(),
                    Description = r.GetAdditionalDescription() 
                })
                .ToList();

            builder.HasData(rolesToSeed);
        }
    }
}