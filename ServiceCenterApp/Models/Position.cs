using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models
{
    public class Position
    {
        [Key]
        public int PositionId { get; set; }
        [Required, MaxLength(100)]
        public string? PositionName { get; set; }
    }
}
