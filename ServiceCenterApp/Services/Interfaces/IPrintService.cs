using ServiceCenterApp.Models;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IPrintService
    {
        void PrintReceptionReceipt(Order order);
    }
}
