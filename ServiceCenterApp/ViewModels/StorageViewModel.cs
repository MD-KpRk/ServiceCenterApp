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

        // --- ЗАПЧАСТИ ---
        private List<SparePart> _allParts;
        public ObservableCollection<SparePart> FilteredParts { get; }

        private SparePart _selectedPart;
        public SparePart SelectedPart { get => _selectedPart; set { _selectedPart = value; OnPropertyChanged(); } }

        public ICommand AddPartCommand { get; }
        public ICommand EditPartCommand { get; }
        public ICommand DeletePartCommand { get; }

        // --- НОВОЕ: УСЛУГИ ---
        private List<Service> _allServices;
        public ObservableCollection<Service> FilteredServices { get; } = new();

        private Service _selectedService;
        public Service SelectedService { get => _selectedService; set { _selectedService = value; OnPropertyChanged(); } }

        public ICommand AddServiceCommand { get; }
        public ICommand EditServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }

        // --- ОБЩЕЕ ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public StorageViewModel(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            FilteredParts = new ObservableCollection<SparePart>();

            // Команды Запчастей
            AddPartCommand = new RelayCommand(ExecuteAddPart);
            EditPartCommand = new RelayCommand(ExecuteEditPart, () => SelectedPart != null);
            DeletePartCommand = new RelayCommand(ExecuteDeletePart, () => SelectedPart != null);

            // Команды Услуг
            AddServiceCommand = new RelayCommand(ExecuteAddService);
            EditServiceCommand = new RelayCommand(ExecuteEditService, () => SelectedService != null);
            DeleteServiceCommand = new RelayCommand(ExecuteDeleteService, () => SelectedService != null);
        }

        public async Task RefreshAsync()
        {
            if (_context.ChangeTracker.HasChanges()) _context.ChangeTracker.Clear();
            SearchText = string.Empty;
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            // 1. Грузим запчасти
            _allParts = await _context.SpareParts
                .AsNoTracking()
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // 2. Грузим услуги
            _allServices = await _context.Services
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allParts == null || _allServices == null) return;

            string lower = SearchText?.ToLower() ?? "";

            // Фильтр запчастей
            var partsResult = _allParts.Where(p =>
                p.Name.ToLower().Contains(lower) ||
                p.PartNumber.ToLower().Contains(lower));

            FilteredParts.Clear();
            foreach (var item in partsResult) FilteredParts.Add(item);

            // Фильтр услуг
            var servicesResult = _allServices.Where(s =>
                s.Name.ToLower().Contains(lower));

            FilteredServices.Clear();
            foreach (var item in servicesResult) FilteredServices.Add(item);
        }

        // --- ЛОГИКА ЗАПЧАСТЕЙ ---
        private async void ExecuteAddPart()
        {
            var vm = new AddEditSparePartViewModel(_context);
            await vm.LoadDataAsync(null);
            var window = new AddEditSparePartWindow(vm);
            if (window.ShowDialog() == true) await LoadDataAsync();
        }

        private async void ExecuteEditPart()
        {
            if (SelectedPart == null) return;
            var vm = new AddEditSparePartViewModel(_context);
            await vm.LoadDataAsync(SelectedPart);
            var window = new AddEditSparePartWindow(vm);
            if (window.ShowDialog() == true) await LoadDataAsync();
        }

        private async void ExecuteDeletePart()
        {
            if (SelectedPart == null) return;
            if (MessageBox.Show($"Удалить '{SelectedPart.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var item = await _context.SpareParts.FindAsync(SelectedPart.PartId);
                    if (item != null) { _context.SpareParts.Remove(item); await _context.SaveChangesAsync(); await LoadDataAsync(); }
                }
                catch { MessageBox.Show("Нельзя удалить, так как используется в заказах.", "Ошибка"); }
            }
        }

        // --- ЛОГИКА УСЛУГ ---
        private void ExecuteAddService()
        {
            var vm = new AddEditServiceViewModel(_context);
            vm.LoadData(null);

            var window = new AddEditServiceWindow(vm);
            if (window.ShowDialog() == true) LoadDataAsync();
        }

        private async void ExecuteEditService()
        {
            if (SelectedService == null) return;
            var vm = new AddEditServiceViewModel(_context);
            vm.LoadData(SelectedService);

            var window = new AddEditServiceWindow(vm);
            if (window.ShowDialog() == true) await LoadDataAsync();
        }

        private async void ExecuteDeleteService()
        {
            if (SelectedService == null) return;
            if (MessageBox.Show($"Удалить услугу '{SelectedService.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    var item = await _context.Services.FindAsync(SelectedService.ServiceId);
                    if (item != null)
                    {
                        _context.Services.Remove(item);
                        await _context.SaveChangesAsync();
                        await LoadDataAsync();
                    }
                }
                catch
                {
                    MessageBox.Show("Нельзя удалить эту услугу, так как она добавлена в заказы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}