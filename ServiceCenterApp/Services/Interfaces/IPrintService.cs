using ServiceCenterApp.Models;
using System.Windows.Documents;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IPrintService
    {
        void PrintReceptionReceipt(Order order);
        FlowDocument CreateReceptionDocument(Order order);
    }
}
