using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models.Lookup
{
    public class DocumentType
    {
        [Key]
        public int DocumentTypeId { get; set; }
        [Required, MaxLength(100)]
        public string? TypeName { get; set; }
    }
}
