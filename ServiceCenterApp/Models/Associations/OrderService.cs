using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models.Associations
{
    public class OrderService
    {
        [Key]
        public int OrderServiceId { get; set; }

        public int OrderId { get; set; }
        public int ServiceId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service? Service { get; set; }
    }
}