using System;

public interface ICurrentUserService
{
    // User mock
    string? CurrentUser { get; }
    bool IsLoggedIn { get; }

    event Action? AuthenticationStateChanged;

    void SetCurrentUser(string user);
    void ClearCurrentUser();
}