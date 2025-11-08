using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;

public class AuthenticationService : IAuthenticationService
{
    private readonly ICurrentUserService _currentUserService;

    public AuthenticationService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    //Login mock. Always Success
    public bool Login(string username, string password)
    {
        string user = "UserMock"; 
        _currentUserService.SetCurrentUser(user); 
        return true;
    }

    public Task<int> LoginAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    public void Logout()
    {
        throw new NotImplementedException();
    }
}