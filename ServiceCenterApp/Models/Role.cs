using ServiceCenterApp.Models.Associations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(100)]
        public string? RoleName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        // Навигационное свойство к связующей таблице
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
