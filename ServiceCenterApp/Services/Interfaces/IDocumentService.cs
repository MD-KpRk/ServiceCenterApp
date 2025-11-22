using ServiceCenterApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>> GetDocumentsByOrderIdAsync(int orderId);
        Task CreateAndSaveDocumentAsync(Order order, int documentTypeId, object documentContent);

        void OpenDocument(Document document);
    }
}