using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IDatabaseHealthService
    {
        Task<bool> CanConnectAsync();
    }
}
