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
                new RolePermission { RoleId = ((int)RoleEnum.Administrator), PermissionId = ((int)PermissionEnum.Orders) }, // Orders
                new RolePermission { RoleId = ((int)RoleEnum.Administrator), PermissionId = ((int)PermissionEnum.Clients) }, // Clients
                new RolePermission { RoleId = ((int)RoleEnum.Administrator), PermissionId = ((int)PermissionEnum.SparePart) }, // SparePart
                new RolePermission { RoleId = ((int)RoleEnum.Administrator), PermissionId = ((int)PermissionEnum.Diagnostic) }, // Diagnostic
                new RolePermission { RoleId = ((int)RoleEnum.Administrator), PermissionId = ((int)PermissionEnum.Payment) }, // Payment
                new RolePermission { RoleId = ((int)RoleEnum.Administrator), PermissionId = ((int)PermissionEnum.Admin) }, // Admin

                new RolePermission { RoleId = ((int)RoleEnum.Receptionist), PermissionId = ((int)PermissionEnum.Orders) }, // Orders
                new RolePermission { RoleId = ((int)RoleEnum.Receptionist), PermissionId = ((int)PermissionEnum.Clients) }, // Clients
                new RolePermission { RoleId = ((int)RoleEnum.Receptionist), PermissionId = ((int)PermissionEnum.Payment) }, // Payment

                new RolePermission { RoleId = ((int)RoleEnum.Technician), PermissionId = ((int)PermissionEnum.Orders) }, // Orders
                new RolePermission { RoleId = ((int)RoleEnum.Technician), PermissionId = ((int)PermissionEnum.SparePart) }, // SparePart
                new RolePermission { RoleId = ((int)RoleEnum.Technician), PermissionId = ((int)PermissionEnum.Diagnostic) }  // Diagnostic
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