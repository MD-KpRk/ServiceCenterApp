using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;

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
    }
}