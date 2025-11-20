using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceCenterApp.Models
{
    public class Position
    {
        [Key]
        public int PositionId { get; set; }

        [Required, MaxLength(100)]
        public string? PositionName { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}