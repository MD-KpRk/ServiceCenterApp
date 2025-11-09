using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using ServiceCenterApp.Models.Lookup;

namespace ServiceCenterApp.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        public int OrderId { get; set; }

        public int EmployeeId { get; set; }

        public int DocumentTypeId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;

        // Навигационные свойства
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("DocumentTypeId")]
        public virtual DocumentType DocumentType { get; set; }
    }
}