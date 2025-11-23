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

        private decimal _costPrice;
        public decimal CostPrice { get => _costPrice; set { _costPrice = value; OnPropertyChanged(); } }
        public decimal SellingPrice { get; set; }

        // Поля
        public int PartId { get; set; }
        public string Name { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        private int _stockQuantity;
        public int StockQuantity { get => _stockQuantity; set { _stockQuantity = value; OnPropertyChanged(); } }

        // Поставщики
        private List<Supplier> _allSuppliers;
        public List<Supplier> AllSuppliers { get => _allSuppliers; private set { _allSuppliers = value; OnPropertyChanged(); } }

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier { get => _selectedSupplier; set { _selectedSupplier = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand AddSupplierCommand { get; }
        public ICommand SupplyCommand { get; }




        public AddEditSparePartViewModel(ApplicationDbContext context)
        {
            _context = context;
            SaveCommand = new RelayCommand(ExecuteSave);
            AddSupplierCommand = new RelayCommand(ExecuteAddSupplier);
            SupplyCommand = new RelayCommand(ExecuteSupply);
        }

        private void ExecuteSupply()
        {
            SupplyWindow window = new SupplyWindow();

            if (window.ShowDialog() == true)
            {
                var vm = window.ViewModel;
                int incomingQty = vm.IncomingQuantity;
                decimal incomingPrice = vm.IncomingPrice;

                decimal oldTotalValue = StockQuantity * CostPrice; 
                decimal newSupplyValue = incomingQty * incomingPrice; 

                int newTotalQty = StockQuantity + incomingQty;

                if (newTotalQty > 0)
                {
                    CostPrice = (oldTotalValue + newSupplyValue) / newTotalQty;
                    CostPrice = Math.Round(CostPrice, 2);
                }
                else
                {
                    CostPrice = incomingPrice;
                }

                StockQuantity = newTotalQty;
            }
        }

        public async Task LoadDataAsync(SparePart partToEdit = null)
        {

            await RefreshSuppliersAsync();

            if (partToEdit != null)
            {
                _isEditMode = true;
                PartId = partToEdit.PartId;
                Name = partToEdit.Name;
                PartNumber = partToEdit.PartNumber;
                Description = partToEdit.Description;

                CostPrice = partToEdit.CostPrice;
                SellingPrice = partToEdit.SellingPrice;

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
                        part.CostPrice = CostPrice;
                        part.SellingPrice = SellingPrice;
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
                        CostPrice = CostPrice,
                        SellingPrice = SellingPrice,
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