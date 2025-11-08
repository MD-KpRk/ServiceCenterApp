using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<int> LoginAsync(string username, string password);
        void Logout();
    }

}
