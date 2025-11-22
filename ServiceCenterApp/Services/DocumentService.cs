using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Views; // Для DocumentViewWindow
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;

namespace ServiceCenterApp.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUserService _currentUserService;

        public DocumentService(IServiceProvider serviceProvider, ICurrentUserService currentUserService)
        {
            _serviceProvider = serviceProvider;
            _currentUserService = currentUserService;
        }

        public async Task<List<Document>> GetDocumentsByOrderIdAsync(int orderId)
        {
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
            string fileName = $"Zakaz_{order.OrderId}_{docTypeName}_{DateTime.Now:ddMMyyyy}.xps";

            byte[] fileBytes;
            if (documentContent is FlowDocument flowDoc)
            {
                flowDoc.PageWidth = 793;
                flowDoc.PageHeight = 1122;
                flowDoc.PagePadding = new Thickness(40);
                flowDoc.ColumnWidth = double.PositiveInfinity;

                using (var ms = new MemoryStream())
                {
                    using (Package package = Package.Open(ms, FileMode.Create))
                    {
                        using (XpsDocument xpsDoc = new XpsDocument(package))
                        {
                            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                            writer.Write(((IDocumentPaginatorSource)flowDoc).DocumentPaginator);
                        }
                    }
                    fileBytes = ms.ToArray();
                }
            }
            else
            {
                throw new ArgumentException("Неверный формат документа");
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var newDoc = new Document
                {
                    OrderId = order.OrderId,
                    EmployeeId = _currentUserService.CurrentUser.EmployeeId,
                    DocumentTypeId = documentTypeId,
                    FileName = fileName,       
                    FileContent = fileBytes,    
                    CreationDate = DateTime.Now
                };

                context.Documents.Add(newDoc);
                await context.SaveChangesAsync();
            }
        }

        public void OpenDocument(Document document)
        {
            if (document?.FileContent == null || document.FileContent.Length == 0)
            {
                MessageBox.Show("Документ пуст или поврежден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), document.FileName ?? "temp.xps");
                File.WriteAllBytes(tempPath, document.FileContent);
                DocumentViewWindow viewer = new DocumentViewWindow(tempPath);
                viewer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть документ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}