using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public int PaymentTypeId { get; set; }

        public int PaymentStatusId { get; set; }

        // Навигационные свойства
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("PaymentTypeId")]
        public virtual PaymentType PaymentType { get; set; }

        [ForeignKey("PaymentStatusId")]
        public virtual PaymentStatus PaymentStatus { get; set; }
    }
}