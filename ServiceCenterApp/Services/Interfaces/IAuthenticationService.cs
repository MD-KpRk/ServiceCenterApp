using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IAuthenticationService
    {
        bool Login(string username, string password);
        //Task<int> LoginAsync(string username, string password);
        void Logout();
    }

}
