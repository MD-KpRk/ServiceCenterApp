using System;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface ICurrentUserService
    {
        // User mock
        string? CurrentUser { get; }
        bool IsLoggedIn { get; }

        event Action? AuthenticationStateChanged;

        void SetCurrentUser(string user);
        void ClearCurrentUser();
    }
}