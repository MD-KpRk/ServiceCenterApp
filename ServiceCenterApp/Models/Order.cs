using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using ServiceCenterApp.Models.Associations;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public int StatusId { get; set; }

        [Required]
        public string ProblemDescription { get; set; }

        public int PriorityId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? Comment { get; set; }

        public int AcceptorId { get; set; }
        public int ClientId { get; set; }
        public int DeviceId { get; set; }

        // Навигационные свойства
        [ForeignKey("StatusId")]
        public virtual OrderStatus Status { get; set; }

        [ForeignKey("PriorityId")]
        public virtual Priority Priority { get; set; }

        [ForeignKey("AcceptorId")]
        public virtual Employee Acceptor { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [ForeignKey("DeviceId")]
        public virtual Device Device { get; set; }

        public virtual DiagnosticReport? DiagnosticReport { get; set; }

        public virtual ICollection<OrderSparePart> OrderSpareParts { get; set; } = new List<OrderSparePart>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}