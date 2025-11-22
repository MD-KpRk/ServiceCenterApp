using System.IO;
using System.Windows;
using System.Windows.Xps.Packaging;

namespace ServiceCenterApp.Views
{
    public partial class DocumentViewWindow : Window
    {
        private XpsDocument _xpsDocument;

        public DocumentViewWindow(string filePath)
        {
            InitializeComponent();

            if (File.Exists(filePath))
            {
                _xpsDocument = new XpsDocument(filePath, FileAccess.Read);
                DocViewer.Document = _xpsDocument.GetFixedDocumentSequence();
            }
            else
            {
                MessageBox.Show("Файл не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _xpsDocument?.Close();
        }
    }
}