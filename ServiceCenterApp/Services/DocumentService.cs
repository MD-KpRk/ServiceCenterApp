using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace ServiceCenterApp.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUserService _currentUserService;
        private readonly string _baseStoragePath;

        public DocumentService(IServiceProvider serviceProvider, ICurrentUserService currentUserService)
        {
            _serviceProvider = serviceProvider;
            _currentUserService = currentUserService;

            _baseStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OrderDocuments");
            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }
        }

        public async Task<List<Document>> GetDocumentsByOrderIdAsync(int orderId)
        {
            // Создаем временный контекст для чтения
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                return await context.Documents
                    .AsNoTracking()
                    .Include(d => d.DocumentType)
                    .Include(d => d.Employee)
                    .Where(d => d.OrderId == orderId)
                    .OrderByDescending(d => d.CreationDate)
                    .ToListAsync();
            }
        }

        public async Task CreateAndSaveDocumentAsync(Order order, int documentTypeId, object documentContent)
        {
            string docTypeName = documentTypeId == 1 ? "Reception" : "WorkAct";
            string fileName = $"Order_{order.OrderId}_{docTypeName}_{DateTime.Now:yyyyMMdd_HHmmss}.xps";
            string fullPath = Path.Combine(_baseStoragePath, fileName);

            if (documentContent is FlowDocument flowDoc)
            {
                flowDoc.PageWidth = 793;
                flowDoc.PageHeight = 1122;
                flowDoc.PagePadding = new System.Windows.Thickness(40);
                flowDoc.ColumnWidth = double.PositiveInfinity;

                if (File.Exists(fullPath)) File.Delete(fullPath);

                using (Package package = Package.Open(fullPath, FileMode.Create))
                {
                    using (XpsDocument xpsDoc = new XpsDocument(package))
                    {
                        XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                        writer.Write(((IDocumentPaginatorSource)flowDoc).DocumentPaginator);
                    }
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var newDoc = new Document
                {
                    OrderId = order.OrderId,
                    EmployeeId = _currentUserService.CurrentUser.EmployeeId,
                    DocumentTypeId = documentTypeId,
                    FilePath = fullPath,
                    CreationDate = DateTime.Now
                };

                context.Documents.Add(newDoc);
                await context.SaveChangesAsync();
            }
        }

        public void OpenDocument(string filePath)
        {
            if (File.Exists(filePath))
            {
                new Views.DocumentViewWindow(filePath).Show();
            }
            else
            {
                throw new FileNotFoundException("Файл документа не найден на диске.");
            }
        }
    }
}