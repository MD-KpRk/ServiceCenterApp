using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models.Lookup
{
    public class Priority
    {
        [Key]
        public int PriorityId { get; set; }
        [Required, MaxLength(50)]
        public string PriorityName { get; set; }
    }
}
