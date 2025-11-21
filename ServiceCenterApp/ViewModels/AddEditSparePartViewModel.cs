using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddEditSparePartViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование детали" : "Новая запчасть";

        // Поля
        public int PartId { get; set; }
        public string Name { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Поставщики
        private List<Supplier> _allSuppliers;
        public List<Supplier> AllSuppliers { get => _allSuppliers; private set { _allSuppliers = value; OnPropertyChanged(); } }

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier { get => _selectedSupplier; set { _selectedSupplier = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand AddSupplierCommand { get; }

        public AddEditSparePartViewModel(ApplicationDbContext context)
        {
            _context = context;
            SaveCommand = new RelayCommand(ExecuteSave);
            AddSupplierCommand = new RelayCommand(ExecuteAddSupplier);
        }

        public async Task LoadDataAsync(SparePart partToEdit = null)
        {
            // 1. Загружаем поставщиков
            await RefreshSuppliersAsync();

            // 2. Если это режим редактирования, заполняем поля
            if (partToEdit != null)
            {
                _isEditMode = true;
                PartId = partToEdit.PartId;
                Name = partToEdit.Name;
                PartNumber = partToEdit.PartNumber;
                Description = partToEdit.Description;
                Price = partToEdit.Price;
                StockQuantity = partToEdit.StockQuantity;

                SelectedSupplier = AllSuppliers.FirstOrDefault(s => s.SupplierId == partToEdit.SupplierId);
            }
            else
            {
                _isEditMode = false;
                if (AllSuppliers.Any()) SelectedSupplier = AllSuppliers.First();
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task RefreshSuppliersAsync()
        {
            AllSuppliers = await _context.Suppliers.ToListAsync();
        }

        private async void ExecuteAddSupplier()
        {
            // Открываем маленькое окно добавления поставщика
            var window = new AddSupplierWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    _context.Suppliers.Add(window.NewSupplier);
                    await _context.SaveChangesAsync();

                    // Обновляем список и выбираем нового
                    await RefreshSuppliersAsync();
                    SelectedSupplier = AllSuppliers.FirstOrDefault(s => s.SupplierId == window.NewSupplier.SupplierId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления поставщика: " + ex.Message);
                }
            }
        }

        private async void ExecuteSave()
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(PartNumber))
            {
                MessageBox.Show("Название и Артикул (Part Number) обязательны.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Выберите поставщика. Если список пуст, добавьте поставщика через '+'.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Price < 0 || StockQuantity < 0)
            {
                MessageBox.Show("Цена и Количество не могут быть отрицательными.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    // Редактирование
                    var part = await _context.SpareParts.FindAsync(PartId);
                    if (part != null)
                    {
                        part.Name = Name;
                        part.PartNumber = PartNumber;
                        part.Description = Description;
                        part.Price = Price;
                        part.StockQuantity = StockQuantity;
                        part.SupplierId = SelectedSupplier.SupplierId;

                        _context.SpareParts.Update(part);
                    }
                }
                else
                {
                    // Создание
                    var newPart = new SparePart
                    {
                        Name = Name,
                        PartNumber = PartNumber,
                        Description = Description,
                        Price = Price,
                        StockQuantity = StockQuantity,
                        SupplierId = SelectedSupplier.SupplierId
                    };
                    _context.SpareParts.Add(newPart);
                }

                await _context.SaveChangesAsync();

                // Закрываем окно
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this) { window.DialogResult = true; window.Close(); break; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}