using ServiceCenterApp.Models.Associations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenterApp.Models
{
    public class SparePart
    {
        [Key]
        public int PartId { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string? PartNumber { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int SupplierId { get; set; }

        // Навигационные свойства
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        public virtual ICollection<OrderSparePart> OrderSpareParts { get; set; } = new List<OrderSparePart>();
    }
}