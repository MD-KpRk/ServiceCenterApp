using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Models;
using System;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface ICurrentUserService
    {
        Employee? CurrentUser { get; }
        bool IsLoggedIn { get; }

        event Action? AuthenticationStateChanged;

        void SetCurrentUser(Employee user);
        void ClearCurrentUser();

        bool HasAllPermissions(params PermissionEnum[] permissions);
    }
}