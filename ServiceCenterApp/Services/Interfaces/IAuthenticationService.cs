using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task CreateAdministratorAsync(string firstName, string surName, string? patronymic, string positionName, string pin);

        Task<bool> LoginAsync(string pin);

        void Logout();
    }

}
