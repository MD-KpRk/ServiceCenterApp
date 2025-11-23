using Microsoft.Extensions.Options;
using ServiceCenterApp.Models;
using ServiceCenterApp.Models.Config;
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
        private readonly OrganizationSettings _orgSettings;

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

        public PrintService(IOptions<OrganizationSettings> options)
        {
            _orgSettings = options.Value; 
        }

        public FlowDocument CreateReceptionDocument(Order order)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Times New Roman");
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

        public FlowDocument CreateWorkCompletionDocument(Order order)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Times New Roman");
            doc.FontSize = 12;
            doc.TextAlignment = TextAlignment.Left;
            doc.LineHeight = 1;

            // --- 1. ЗАГОЛОВОК ---
            doc.Blocks.Add(new Paragraph(new Run($"АКТ ВЫПОЛНЕННЫХ РАБОТ № {order.OrderId}"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            });

            doc.Blocks.Add(new Paragraph(new Run($"от {DateTime.Now:dd MMMM yyyy} г."))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // СТОРОНЫ (ЗАКАЗЧИК / ИСПОЛНИТЕЛЬ) ---
            Grid partiesGrid = new Grid();
            partiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            partiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) }); // Отступ
            partiesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel executorPanel = new StackPanel();
            executorPanel.Children.Add(new TextBlock(new Run("ИСПОЛНИТЕЛЬ:")) { FontWeight = FontWeights.Bold });
            executorPanel.Children.Add(new TextBlock(new Run(_orgSettings.Name)));
            executorPanel.Children.Add(new TextBlock(new Run(_orgSettings.Address)));
            executorPanel.Children.Add(new TextBlock(new Run(_orgSettings.UNP)));
            executorPanel.Children.Add(new TextBlock(new Run($"Тел: {_orgSettings.Phone}")));

            Grid.SetColumn(executorPanel, 0);
            partiesGrid.Children.Add(executorPanel);

            string clientName = $"{order.Client?.SurName} {order.Client?.FirstName} {order.Client?.Patronymic}".Trim();
            string phone = order.Client?.PhoneNumber ?? "";

            StackPanel clientPanel = new StackPanel();
            clientPanel.Children.Add(new TextBlock(new Run("ЗАКАЗЧИК:")) { FontWeight = FontWeights.Bold });
            clientPanel.Children.Add(new TextBlock(new Run(clientName)));
            clientPanel.Children.Add(new TextBlock(new Run($"Тел: {phone}")));

            Grid.SetColumn(clientPanel, 2);
            partiesGrid.Children.Add(clientPanel);

            doc.Blocks.Add(new BlockUIContainer(partiesGrid) { Margin = new Thickness(0, 0, 0, 20) });

            Paragraph devicePara = new Paragraph();
            devicePara.Inlines.Add(new Run("Оборудование: ") { FontWeight = FontWeights.Bold });
            devicePara.Inlines.Add(new Run($"{order.Device?.DeviceType} {order.Device?.Brand} {order.Device?.Model}, S/N: {order.Device?.SerialNumber}"));
            doc.Blocks.Add(devicePara);

            Grid tableGrid = new Grid();
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); 
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) }); 
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); 

            int currentRow = 0;
            AddTableHeader(tableGrid, ref currentRow, "№", "Наименование работ (услуг), запчастей", "Ед.", "Кол.", "Цена", "Сумма");

            decimal totalSum = 0;
            int index = 1;

 
            if (order.OrderServices != null)
            {
                foreach (var service in order.OrderServices)
                {
                    decimal sum = service.Price;

                    totalSum += sum;

                    AddTableRow(tableGrid, ref currentRow,
                        index++.ToString(),
                        service.Service?.Name ?? "Услуга",
                        "усл.",
                        "1", 
                        service.Price.ToString("N2"),
                        sum.ToString("N2"));
                }
            }


            if (order.OrderSpareParts != null)
            {
                foreach (var part in order.OrderSpareParts)
                {
                    decimal sum = part.SalePrice * part.Quantity;
                    totalSum += sum;
                    AddTableRow(tableGrid, ref currentRow,
                        index++.ToString(),
                        part.SparePart?.Name ?? "Запчасть",
                        "шт.",
                        part.Quantity.ToString(),
                        part.SalePrice.ToString("N2"),
                        sum.ToString("N2"));
                }
            }

            doc.Blocks.Add(new BlockUIContainer(tableGrid) { Margin = new Thickness(0, 10, 0, 20) });

            Paragraph totalPara = new Paragraph();
            totalPara.TextAlignment = TextAlignment.Right;
            totalPara.FontSize = 14;
            totalPara.Inlines.Add(new Run("ИТОГО К ОПЛАТЕ: ") { FontWeight = FontWeights.Bold });
            totalPara.Inlines.Add(new Run($"{totalSum:N2} (Белорусских рублей)"));
            doc.Blocks.Add(totalPara);

            string warrantyString;
            if (order.WarrantyDays > 0)
            {
                DateTime endDate = DateTime.Now.AddDays(order.WarrantyDays);
                warrantyString = $"Срок гарантии: {order.WarrantyDays} дн. (Действительна до {endDate:dd.MM.yyyy}).";
            }
            else
            {
                warrantyString = "Гарантия на выполненные работы не предоставляется (Тестовый режим / Чистка / ПО).";
            }

            doc.Blocks.Add(new Paragraph(new Run("ГАРАНТИЙНЫЕ ОБЯЗАТЕЛЬСТВА:")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 5) });

            Paragraph warrantyText = new Paragraph() { FontSize = 10 };

            warrantyText.Inlines.Add(new Run($"1. {warrantyString}\n") { FontWeight = FontWeights.Bold });

            warrantyText.Inlines.Add(new Run("2. Гарантия распространяется только на заявленную неисправность и замененные детали.\n"));
            warrantyText.Inlines.Add(new Run("3. Гарантия аннулируется при обнаружении следов вскрытия, попадания влаги, механических повреждений или нарушения правил эксплуатации.\n"));
            warrantyText.Inlines.Add(new Run("4. Заказчик претензий по объему, качеству и срокам выполнения работ не имеет. Оборудование проверено в присутствии Заказчика."));

            doc.Blocks.Add(warrantyText);

            // --- 7. ПОДПИСИ ---
            Grid signGrid = new Grid();
            signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel leftSign = new StackPanel();
            leftSign.Children.Add(new TextBlock(new Run("Исполнитель (Мастер):")) { Margin = new Thickness(0, 0, 0, 30) });
            leftSign.Children.Add(new TextBlock(new Run("_______________________ / М.П.")));
            Grid.SetColumn(leftSign, 0);
            signGrid.Children.Add(leftSign);

            StackPanel rightSign = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Right };
            rightSign.Children.Add(new TextBlock(new Run("Заказчик:")) { Margin = new Thickness(0, 0, 0, 30) });
            rightSign.Children.Add(new TextBlock(new Run("_______________________ (Подпись)")));
            Grid.SetColumn(rightSign, 1);
            signGrid.Children.Add(rightSign);

            doc.Blocks.Add(new BlockUIContainer(signGrid) { Margin = new Thickness(0, 40, 0, 0) });

            return doc;
        }

        public FlowDocument CreateWarrantyTicketDocument(Order order)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Arial");
            doc.FontSize = 11; // Шрифт чуть поменьше для компактности
            doc.TextAlignment = TextAlignment.Left;
            doc.LineHeight = 1;
            doc.PageWidth = 793;
            doc.PageHeight = 560;
            doc.PagePadding = new Thickness(30, 20, 30, 20); // Поля поменьше
            doc.ColumnWidth = double.PositiveInfinity;

            Border mainBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(15),
                Background = Brushes.White
            };

            StackPanel content = new StackPanel();
            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var companyBlock = new TextBlock(new Run(_orgSettings.Name))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleBlock = new TextBlock(new Run("ГАРАНТИЙНЫЙ ТАЛОН"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Black,
                TextAlignment = TextAlignment.Right,
                TextDecorations = TextDecorations.Underline,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(companyBlock, 0);
            Grid.SetColumn(titleBlock, 1);
            headerGrid.Children.Add(companyBlock);
            headerGrid.Children.Add(titleBlock);

            content.Children.Add(headerGrid);
            content.Children.Add(new Separator { Margin = new Thickness(0, 5, 0, 10), Background = Brushes.Black });

            Grid infoGrid = new Grid();
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = 0;
            void AddCompactRow(string label, string value)
            {
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var lbl = new TextBlock(new Run(label)) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 2, 0, 2) };
                var val = new TextBlock(new Run(value)) { Margin = new Thickness(0, 2, 0, 2), TextWrapping = TextWrapping.Wrap };

                var line = new Border { BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = Brushes.LightGray, Opacity = 0.5 };

                Grid.SetRow(lbl, row); Grid.SetColumn(lbl, 0);
                Grid.SetRow(val, row); Grid.SetColumn(val, 1);
                Grid.SetRow(line, row); Grid.SetColumnSpan(line, 2);

                infoGrid.Children.Add(line);
                infoGrid.Children.Add(lbl);
                infoGrid.Children.Add(val);
                row++;
            }

            AddCompactRow("Талон № / Дата:", $"{order.OrderId} от {DateTime.Now:dd.MM.yyyy}");
            AddCompactRow("Устройство:", $"{order.Device?.Brand} {order.Device?.Model}");
            AddCompactRow("Серийный номер:", order.Device?.SerialNumber ?? "-");

            string works = "Ремонт устройства";
            if (order.OrderServices != null && order.OrderServices.Any())
                works = string.Join(", ", order.OrderServices.Select(s => s.Service?.Name));
            AddCompactRow("Выполненные работы:", works);

            content.Children.Add(infoGrid);

            int days = order.WarrantyDays;
            string dateEnd = days > 0 ? DateTime.Now.AddDays(days).ToString("dd.MM.yyyy") : "—";

            Border warrantyBox = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Black,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 15, 0, 15),
                Background = Brushes.WhiteSmoke
            };

            StackPanel warrantyStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            warrantyStack.Children.Add(new TextBlock(new Run("ГАРАНТИЯ: ")) { FontWeight = FontWeights.Bold, FontSize = 14 });
            warrantyStack.Children.Add(new TextBlock(new Run($"{days} дней. ")) { FontWeight = FontWeights.Black, FontSize = 14 });
            if (days > 0)
            {
                warrantyStack.Children.Add(new TextBlock(new Run($"Действительна до: {dateEnd}")) { FontSize = 14, Margin = new Thickness(10, 0, 0, 0), TextDecorations = TextDecorations.Underline });
            }

            warrantyBox.Child = warrantyStack;
            content.Children.Add(warrantyBox);

            content.Children.Add(new TextBlock(new Run(
                "1. Гарантия распространяется только на замененные запчасти и выполненные работы.\n" +
                "2. При попадании влаги, механических повреждениях или вскрытии пломб гарантия аннулируется.\n" +
                "3. Гарантийный талон действителен только при наличии печати сервисного центра."))
            { FontSize = 9, FontStyle = FontStyles.Italic, Margin = new Thickness(0, 0, 0, 15) });

            // 5. ПОДПИСИ
            Grid signGrid = new Grid();
            signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            signGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var stampPlace = new TextBlock(new Run("М.П.")) { FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(20, 0, 0, 0) };
            var signLine = new TextBlock(new Run("Мастер: __________________")) { HorizontalAlignment = HorizontalAlignment.Right };

            Grid.SetColumn(stampPlace, 0);
            Grid.SetColumn(signLine, 1);
            signGrid.Children.Add(stampPlace);
            signGrid.Children.Add(signLine);

            content.Children.Add(signGrid);

            content.Children.Add(new TextBlock(new Run($"{_orgSettings.Address}, тел. {_orgSettings.Phone}"))
            {
                FontSize = 9,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            });

            mainBorder.Child = content;
            doc.Blocks.Add(new BlockUIContainer(mainBorder));

            StackPanel cutLinePanel = new StackPanel { Margin = new Thickness(0, 20, 0, 0) };

            doc.Blocks.Add(new BlockUIContainer(cutLinePanel));

            return doc;
        }


        private void AddTableHeader(Grid grid, ref int rowIndex, params string[] headers)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int i = 0; i < headers.Length; i++)
            {
                Border border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Padding = new Thickness(5),
                    Background = Brushes.LightGray // Слегка серый фон
                };
                border.Child = new TextBlock(new Run(headers[i])) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };

                Grid.SetRow(border, rowIndex);
                Grid.SetColumn(border, i);
                grid.Children.Add(border);
            }
            rowIndex++;
        }

        // Хелпер для обычной строки (с рамками)
        private void AddTableRow(Grid grid, ref int rowIndex, params string[] values)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int i = 0; i < values.Length; i++)
            {
                Border border = new Border
                {
                    // Делаем общую границу. Чтобы линии не удваивались, используем (1,0,1,1) или Margin -1
                    // Простой способ: Thickness(1) и наложение Grid.
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Padding = new Thickness(5)
                };

                var textBlock = new TextBlock(new Run(values[i])) { TextWrapping = TextWrapping.Wrap };

                // Выравнивание для цифр (Кол, Цена, Сумма) - вправо
                if (i >= 2) textBlock.TextAlignment = TextAlignment.Right;
                // Выравнивание для № и Ед - центр
                if (i == 0 || i == 2) textBlock.TextAlignment = TextAlignment.Center;

                border.Child = textBlock;

                Grid.SetRow(border, rowIndex);
                Grid.SetColumn(border, i);
                grid.Children.Add(border);
            }
            rowIndex++;
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