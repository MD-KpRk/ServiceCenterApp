using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Helpers;
using ServiceCenterApp.Models.Lookup;
using System.ComponentModel;


namespace ServiceCenterApp.Data.Configurations
{
    public enum PermissionEnum
    {
        [Description("Доступ к модулю Заявок")]
        Orders = 1,

        [Description("Доступ к модулю Клиентов")]
        Clients = 2,

        [Description("Доступ к модулю Склада")]
        SparePart = 3,

        [Description("Доступ к модулю Диагностики и ремонта")]
        Diagnostic = 4,

        [Description("Доступ к модулю Финансы и Платежи")]
        Payment = 5,

        [Description("Доступ к модулю Администрирование")]
        Admin = 6
    }

    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasIndex(p => p.PermissionKey).IsUnique();

            List<Permission> permissionsToSeed = Enum.GetValues(typeof(PermissionEnum))
                .Cast<PermissionEnum>()
                .Select(p => new Permission
                {
                    PermissionId = (int)p,
                    PermissionKey = p.ToString(),
                    Description = p.GetDescription()
                })
                .ToList();

            builder.HasData(permissionsToSeed);
        }
    }
}