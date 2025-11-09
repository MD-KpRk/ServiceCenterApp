using ServiceCenterApp.Models.Lookup;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Models.Associations
{
    public class RolePermission
    {
        // Составной первичный ключ
        public int RoleId { get; set; }
        public int PermissionId { get; set; }


        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission? Permission { get; set; }
    }
}
