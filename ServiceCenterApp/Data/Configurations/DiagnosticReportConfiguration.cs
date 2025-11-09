using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCenterApp.Models;

namespace ServiceCenterApp.Data.Configurations
{
    public class DiagnosticReportConfiguration : IEntityTypeConfiguration<DiagnosticReport>
    {
        public void Configure(EntityTypeBuilder<DiagnosticReport> builder)
        {
            // Дата диагностики по умолчанию - текущая дата
            builder.Property(dr => dr.DiagnosisDate).HasDefaultValueSql("GETDATE()");

            // Настройка связи "один-к-одному" с Заказом
            builder.HasOne(dr => dr.Order)
                   .WithOne(o => o.DiagnosticReport)
                   .HasForeignKey<DiagnosticReport>(dr => dr.OrderId);

            // Запрет на удаление Мастера, если он автор отчетов
            builder.HasOne(dr => dr.Master)
                   .WithMany(e => e.AuthoredDiagnosticReports)
                   .HasForeignKey(dr => dr.MasterId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}