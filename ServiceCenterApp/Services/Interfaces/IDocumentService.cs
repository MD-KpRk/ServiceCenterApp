using ServiceCenterApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceCenterApp.Services.Interfaces
{
    public interface IDocumentService
    {
        // Возвращает список документов для конкретного заказа
        Task<List<Document>> GetDocumentsByOrderIdAsync(int orderId);

        // Создает файл (XPS), сохраняет его на диск и пишет запись в БД
        Task CreateAndSaveDocumentAsync(Order order, int documentTypeId, object documentContent);

        // Открывает файл в программе по умолчанию
        void OpenDocument(string filePath);
    }
}