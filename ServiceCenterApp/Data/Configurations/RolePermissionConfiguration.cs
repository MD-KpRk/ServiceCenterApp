using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models.Associations;
using System.Reflection.Emit;

namespace ServiceCenterApp.Data.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            builder.HasData(
                // Admin (RoleId = 1) -
                new RolePermission { RoleId = 1, PermissionId = 1 }, // Orders
                new RolePermission { RoleId = 1, PermissionId = 2 }, // Clients
                new RolePermission { RoleId = 1, PermissionId = 3 }, // SparePart
                new RolePermission { RoleId = 1, PermissionId = 4 }, // Diagnostic
                new RolePermission { RoleId = 1, PermissionId = 5 }, // Payment
                new RolePermission { RoleId = 1, PermissionId = 6 }, // Admin

                // Приемщик (RoleId = 2)
                new RolePermission { RoleId = 2, PermissionId = 1 }, // Orders
                new RolePermission { RoleId = 2, PermissionId = 2 }, // Clients
                new RolePermission { RoleId = 2, PermissionId = 5 }, // Payment

                // Мастер (RoleId = 3)
                new RolePermission { RoleId = 3, PermissionId = 1 }, // Orders
                new RolePermission { RoleId = 3, PermissionId = 3 }, // SparePart
                new RolePermission { RoleId = 3, PermissionId = 4 }  // Diagnostic
            );

            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            builder.HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId);
        }
    }
}