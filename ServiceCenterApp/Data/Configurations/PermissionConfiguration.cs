using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Lookup;


namespace ServiceCenterApp.Data.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasIndex(p => p.PermissionKey).IsUnique();

            builder.HasData(
                new Permission { PermissionId = 1, PermissionKey = "Orders", Description = "Доступ к модулю заказов" },
                new Permission { PermissionId = 2, PermissionKey = "Clients", Description = "Доступ к модулю клиентов" },
                new Permission { PermissionId = 3, PermissionKey = "SparePart", Description = "Доступ к модулю склада и запчастей" },
                new Permission { PermissionId = 4, PermissionKey = "Diagnostic", Description = "Доступ к модулю диагностики и ремонта" },
                new Permission { PermissionId = 5, PermissionKey = "Payment", Description = "Доступ к модулю Финансы и Платежи" },
                new Permission { PermissionId = 6, PermissionKey = "Admin", Description = "Доступ к модулю Администрирование" }
            );
        }
    }
}