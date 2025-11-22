using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceCenterApp.Models.Associations;

namespace ServiceCenterApp.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; } // Название (Диагностика, Чистка)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BasePrice { get; set; } // Базовая цена по прайсу

        // Связь
        public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
    }
}