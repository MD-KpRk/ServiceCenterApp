using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models
{
    public class DiagnosticReport
    {
        [Key]
        public int ReportId { get; set; }

        public int OrderId { get; set; }

        public DateTime DiagnosisDate { get; set; } = DateTime.Now;

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? Recommendations { get; set; }

        public int MasterId { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("MasterId")]
        public virtual Employee? Master { get; set; }
    }
}