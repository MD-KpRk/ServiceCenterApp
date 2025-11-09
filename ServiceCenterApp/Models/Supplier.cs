using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceCenterApp.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(128)]
        public string? Contacts { get; set; }

        public string? Details { get; set; }

        // Навигационное свойство: один поставщик может поставлять много запчастей
        public virtual ICollection<SparePart> SpareParts { get; set; } = new List<SparePart>();
    }
}