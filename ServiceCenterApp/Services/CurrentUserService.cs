using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Windows;

namespace ServiceCenterApp.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Employee? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        public event Action? AuthenticationStateChanged;

        public void SetCurrentUser(Employee user)
        {
            CurrentUser = user;
            AuthenticationStateChanged?.Invoke();
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
            AuthenticationStateChanged?.Invoke();
        }

        public bool HasAllPermissions(params PermissionEnum[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
            {
                return true;
            }

            if (CurrentUser?.Role?.RolePermissions == null)
            {
                return false;
            }

            HashSet<string?> userPermissionKeys = CurrentUser.Role.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission.PermissionKey)
                .ToHashSet();

            foreach (PermissionEnum requiredPermission in permissions)
            {
                if (!userPermissionKeys.Contains(requiredPermission.ToString()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}