using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(r => r.RoleName).IsUnique();

        builder.HasData(
            new Role { RoleId = 1, RoleName = "Администратор", Description = "Полный доступ" },
            new Role { RoleId = 2, RoleName = "Приемщик", Description = "Регистрация и выдача заказов" },
            new Role { RoleId = 3, RoleName = "Мастер", Description = "Диагностика и ремонт" }
        );
    }
}