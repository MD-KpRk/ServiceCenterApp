// ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Models;
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

        // --- Helper Table ---
        public DbSet<OrderSparePart> OrderSpareParts { get; set; }

        // --- Lookup Tables ---
        public DbSet<Role> Roles { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UNIQUE CONSTRAINTS
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Device>()
                .HasIndex(e => e.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Login)
                .IsUnique();

            modelBuilder.Entity<SparePart>()
                .HasIndex(e => e.PartNumber)
                .IsUnique();

            modelBuilder.Entity<Supplier>()
                .HasIndex(e => e.Name)
                .IsUnique();

            modelBuilder.Entity<Role>().HasIndex(e => e.RoleName).IsUnique();
            modelBuilder.Entity<Position>().HasIndex(e => e.PositionName).IsUnique();
            modelBuilder.Entity<OrderStatus>().HasIndex(e => e.StatusName).IsUnique();
            modelBuilder.Entity<Priority>().HasIndex(e => e.PriorityName).IsUnique();
            modelBuilder.Entity<PaymentType>().HasIndex(e => e.TypeName).IsUnique();
            modelBuilder.Entity<PaymentStatus>().HasIndex(e => e.StatusName).IsUnique();
            modelBuilder.Entity<DocumentType>().HasIndex(e => e.TypeName).IsUnique();

            // DEFAULT VALUES

            modelBuilder.Entity<Device>()
                .Property(e => e.WarrantyStatus)
                .HasDefaultValue(false); 

            modelBuilder.Entity<SparePart>()
                .Property(e => e.StockQuantity)
                .HasDefaultValue(0);

            modelBuilder.Entity<OrderSparePart>()
                .Property(e => e.Quantity)
                .HasDefaultValue(1);

            modelBuilder.Entity<Order>()
                .Property(e => e.RegistrationDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<DiagnosticReport>()
                .Property(e => e.DiagnosisDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Payment>()
                .Property(e => e.PaymentDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Document>()
                .Property(e => e.CreationDate)
                .HasDefaultValueSql("GETDATE()");

            // DECIMAL PRECISION
            modelBuilder.Entity<SparePart>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10, 2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10, 2)");

            // RELATIONSHIPS & ON DELETE

            modelBuilder.Entity<OrderSparePart>()
                .HasKey(osp => new { osp.OrderId, osp.PartId });

            // Связь один-к-одному между Order и DiagnosticReport
            modelBuilder.Entity<Order>()
                .HasOne(o => o.DiagnosticReport)
                .WithOne(dr => dr.Order)
                .HasForeignKey<DiagnosticReport>(dr => dr.OrderId);

            // Запрет на удаление Клиента, если у него есть Заказы
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Запрет на удаление Сотрудника (приемщика), если он принял Заказы
            modelBuilder.Entity<Order>()
               .HasOne(o => o.Acceptor)
               .WithMany(e => e.AcceptedOrders)
               .HasForeignKey(o => o.AcceptorId)
               .OnDelete(DeleteBehavior.Restrict);

            // Запрет на удаление Сотрудника (мастера), если он автор Отчетов
            modelBuilder.Entity<DiagnosticReport>()
               .HasOne(dr => dr.Master)
               .WithMany(e => e.AuthoredDiagnosticReports)
               .HasForeignKey(dr => dr.MasterId)
               .OnDelete(DeleteBehavior.Restrict); 

            // Запрет на удаление Запчасти, если она используется в Заказах
            modelBuilder.Entity<OrderSparePart>()
               .HasOne(os => os.SparePart)
               .WithMany(sp => sp.OrderSpareParts)
               .HasForeignKey(os => os.PartId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}