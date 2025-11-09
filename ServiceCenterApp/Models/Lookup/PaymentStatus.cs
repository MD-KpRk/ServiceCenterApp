using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models.Lookup
{
    public class PaymentStatus
    {
        [Key]
        public int PaymentStatusId { get; set; }
        [Required, MaxLength(50)]
        public string StatusName { get; set; }
    }
}
