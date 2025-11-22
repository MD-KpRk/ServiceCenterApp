using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ServiceCenterApp.Services
{
    public class PrintService : IPrintService
    {
        public void PrintReceptionReceipt(Order order)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                double pageWidth = printDialog.PrintableAreaWidth;
                double pageHeight = printDialog.PrintableAreaHeight;

                FlowDocument doc = CreateReceptionDocument(order);

                doc.PageWidth = pageWidth;
                doc.PageHeight = pageHeight;
                doc.PagePadding = new Thickness(40);
                doc.ColumnWidth = double.PositiveInfinity; // Запрет колонок

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, $"Заказ_{order.OrderId}");
            }
        }

        public FlowDocument CreateReceptionDocument(Order order)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Arial");
            doc.FontSize = 12;
            doc.TextAlignment = TextAlignment.Left;

            // --- ЗАГОЛОВОК ---
            doc.Blocks.Add(new Paragraph(new Run("КВИТАНЦИЯ О ПРИЕМЕ"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Заказ № {order.OrderId}"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            });

            doc.Blocks.Add(new Paragraph(new Run($"Дата приема: {order.RegistrationDate:dd.MM.yyyy HH:mm}"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            Grid grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Данные
            string clientInfo = $"{order.Client?.SurName} {order.Client?.FirstName} {order.Client?.Patronymic}".Trim();
            string deviceName = $"{order.Device?.DeviceType} {order.Device?.Brand} {order.Device?.Model}".Trim();

            int currentRow = 0;
            AddGridRow(grid, ref currentRow, "Клиент:", string.IsNullOrEmpty(clientInfo) ? "Не указан" : clientInfo);
            AddGridRow(grid, ref currentRow, "Телефон:", order.Client?.PhoneNumber ?? "—");
            AddGridRow(grid, ref currentRow, "Устройство:", string.IsNullOrEmpty(deviceName) ? "Не указано" : deviceName);
            AddGridRow(grid, ref currentRow, "Серийный номер:", order.Device?.SerialNumber ?? "—");
            AddGridRow(grid, ref currentRow, "Неисправность:", order.ProblemDescription ?? "—");
            AddGridRow(grid, ref currentRow, "Комплектация / Коммент:", order.Comment ?? "—");

            // Оборачиваем Grid в контейнер, чтобы вставить в документ
            BlockUIContainer container = new BlockUIContainer(grid);
            doc.Blocks.Add(container);

            // --- ЛИНИЯ РАЗДЕЛИТЕЛЬ ---
            doc.Blocks.Add(new Paragraph()
            {
                BorderThickness = new Thickness(0, 1, 0, 0),
                BorderBrush = Brushes.Black,
                Margin = new Thickness(0, 20, 0, 10)
            });

            // --- УСЛОВИЯ (Мелкий текст) ---
            doc.Blocks.Add(new Paragraph(new Run(
                "УСЛОВИЯ РЕМОНТА:\n" +
                "1. Аппарат принимается без разборки и проверки скрытых дефектов.\n" +
                "2. СЦ не несет ответственности за потерю данных. Сохраните важные данные заранее.\n" +
                "3. При отказе от ремонта диагностика оплачивается отдельно."))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 30)
            });

            // --- ПОДПИСИ (Тоже через Grid для ровности) ---
            Grid signGrid = new Grid();
            signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Левая подпись
            StackPanel leftSign = new StackPanel();
            leftSign.Children.Add(new TextBlock(new Run("Сдал (Клиент):")) { Margin = new Thickness(0, 0, 0, 30) });
            leftSign.Children.Add(new TextBlock(new Run("_______________________")));
            Grid.SetColumn(leftSign, 0);
            signGrid.Children.Add(leftSign);

            // Правая подпись
            StackPanel rightSign = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Right };
            rightSign.Children.Add(new TextBlock(new Run("Принял (Мастер):")) { Margin = new Thickness(0, 0, 0, 30) });
            rightSign.Children.Add(new TextBlock(new Run("_______________________")));
            Grid.SetColumn(rightSign, 1);
            signGrid.Children.Add(rightSign);

            doc.Blocks.Add(new BlockUIContainer(signGrid));

            return doc;
        }

        // Вспомогательный метод для добавления строк в Grid
        private void AddGridRow(Grid grid, ref int rowIndex, string label, string value)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock txtLabel = new TextBlock(new Run(label))
            {
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(0, 5, 10, 5),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(txtLabel, rowIndex);
            Grid.SetColumn(txtLabel, 0);
            grid.Children.Add(txtLabel);

            TextBlock txtValue = new TextBlock(new Run(value))
            {
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(0, 5, 0, 5),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(txtValue, rowIndex);
            Grid.SetColumn(txtValue, 1);
            grid.Children.Add(txtValue);

            Border line = new Border()
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = Brushes.LightGray,
                Opacity = 0.5
            };
            Grid.SetRow(line, rowIndex);
            Grid.SetColumnSpan(line, 2);
            grid.Children.Add(line);

            rowIndex++;
        }

    }

}