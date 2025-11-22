using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class StorageViewModel : BaseViewModel, IRefreshable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        // Коллекция запчастей
        private List<SparePart> _allParts;
        public ObservableCollection<SparePart> FilteredParts { get; }

        private SparePart _selectedPart;
        public SparePart SelectedPart { get => _selectedPart; set { _selectedPart = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public ICommand AddPartCommand { get; }
        public ICommand EditPartCommand { get; }
        public ICommand DeletePartCommand { get; }

        public StorageViewModel(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            FilteredParts = new ObservableCollection<SparePart>();

            AddPartCommand = new RelayCommand(ExecuteAddPart);
            EditPartCommand = new RelayCommand(ExecuteEditPart, () => SelectedPart != null);
            DeletePartCommand = new RelayCommand(ExecuteDeletePart, () => SelectedPart != null);
        }

        public async Task RefreshAsync()
        {
            if (_context.ChangeTracker.HasChanges()) _context.ChangeTracker.Clear();
            SearchText = string.Empty;
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            // Подгружаем данные с Поставщиком
            _allParts = await _context.SpareParts
                .AsNoTracking()
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allParts == null) return;

            var result = _allParts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lower = SearchText.ToLower();
                result = result.Where(p =>
                    p.Name.ToLower().Contains(lower) ||
                    p.PartNumber.ToLower().Contains(lower));
            }

            FilteredParts.Clear();
            foreach (var item in result) FilteredParts.Add(item);
        }

        private async void ExecuteAddPart()
        {
            // Создаем новую ViewModel для окна, передаем тот же контекст (Transient)
            var vm = new AddEditSparePartViewModel(_context);
            await vm.LoadDataAsync(null); // Режим создания

            var window = new AddEditSparePartWindow(vm);
            if (window.ShowDialog() == true)
            {
                await LoadDataAsync(); // Перезагружаем таблицу
            }
        }

        private async void ExecuteEditPart()
        {
            if (SelectedPart == null) return;

            var vm = new AddEditSparePartViewModel(_context);
            await vm.LoadDataAsync(SelectedPart);

            var window = new AddEditSparePartWindow(vm);
            if (window.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private async void ExecuteDeletePart()
        {
            if (SelectedPart == null) return;

            var res = MessageBox.Show($"Вы уверены, что хотите удалить '{SelectedPart.Name}'?\nЭто действие нельзя отменить.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    var partToDelete = await _context.SpareParts.FindAsync(SelectedPart.PartId);
                    if (partToDelete != null)
                    {
                        _context.SpareParts.Remove(partToDelete);
                        await _context.SaveChangesAsync();
                        await LoadDataAsync();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Нельзя удалить эту деталь, так как она уже используется в заказах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}