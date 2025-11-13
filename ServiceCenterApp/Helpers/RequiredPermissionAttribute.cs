using ServiceCenterApp.Data.Configurations;
using System;

namespace ServiceCenterApp.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequiredPermissionAttribute : Attribute
    {
        public PermissionEnum[] RequiredPermissions { get; }

        public RequiredPermissionAttribute(params PermissionEnum[] requiredPermissions)
        {
            RequiredPermissions = requiredPermissions ?? throw new ArgumentNullException(nameof(requiredPermissions));
        }
    }
}