using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Associations;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- Main Tables ---
        public DbSet<Client> Clients { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<DiagnosticReport> DiagnosticReports { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        // --- Association Helper Tables ---
        public DbSet<OrderSparePart> OrderSpareParts { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // --- Lookup Tables ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}