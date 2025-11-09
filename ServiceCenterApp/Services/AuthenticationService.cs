using ServiceCenterApp.Data;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace ServiceCenterApp.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _dbcontext;

        public AuthenticationService(ApplicationDbContext dbcontext, ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            _dbcontext = dbcontext;
        }

        //Login mock. Always Success
        public bool Login(string username, string password)
        {
            string user = "UserMock";
            _currentUserService.SetCurrentUser(user);
            MessageBox.Show(_dbcontext.Roles.First().RoleName);
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
}