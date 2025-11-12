using ServiceCenterApp.Data;
using ServiceCenterApp.Services.Interfaces;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services
{
    public class DatabaseHealthService : IDatabaseHealthService
    {
        private readonly ApplicationDbContext _dbContext;

        public DatabaseHealthService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CanConnectAsync()
        {
            try
            {
                return await _dbContext.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}