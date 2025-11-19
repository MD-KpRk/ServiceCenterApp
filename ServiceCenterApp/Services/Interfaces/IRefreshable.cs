using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IRefreshable
    {
        Task RefreshAsync();
    }
}