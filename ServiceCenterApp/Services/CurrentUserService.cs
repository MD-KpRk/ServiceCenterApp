using ServiceCenterApp.Services.Interfaces;
using System;

public class CurrentUserService : ICurrentUserService
{
    public string? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public event Action? AuthenticationStateChanged;

    public void SetCurrentUser(string user)
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