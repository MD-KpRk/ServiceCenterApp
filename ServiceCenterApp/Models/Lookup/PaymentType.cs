using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models.Lookup
{
    public class PaymentType
    {
        [Key]
        public int PaymentTypeId { get; set; }
        [Required, MaxLength(50)]
        public string? TypeName { get; set; }
    }
}
