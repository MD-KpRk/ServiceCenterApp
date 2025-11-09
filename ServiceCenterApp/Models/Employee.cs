using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection.Metadata;
using System.Windows.Controls;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        public int PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Login { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; }

        [MaxLength(255)]
        public string? ContactInfo { get; set; }

        public int RoleId { get; set; }

        // Навигационные свойства
        [ForeignKey("PositionId")]
        public virtual Position Position { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        public virtual ICollection<Order> AcceptedOrders { get; set; } = new List<Order>();
        public virtual ICollection<DiagnosticReport> AuthoredDiagnosticReports { get; set; } = new List<DiagnosticReport>();
        public virtual ICollection<Document> AuthoredDocuments { get; set; } = new List<Document>();
    }
}