using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models.Lookup
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        //Backend key
        [Required]
        [MaxLength(32)]
        public string? PermissionKey { get; set; }

        // UI Title
        [MaxLength(128)]
        public string? Description { get; set; }
    }
}
