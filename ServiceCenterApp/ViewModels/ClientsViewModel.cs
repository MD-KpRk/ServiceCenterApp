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
    public class ClientsViewModel : BaseViewModel, IRefreshable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        #region Список (Left Panel)
        private List<ClientListItemViewModel> _allClients = new List<ClientListItemViewModel>();
        public ObservableCollection<ClientListItemViewModel> FilteredClients { get; }

        public ICommand SelectClientCommand { get; }
        public ICommand AddClientCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }
        #endregion

        #region Детали (Right Panel)
        private Client _selectedClientDetails;
        public Client SelectedClientDetails
        {
            get => _selectedClientDetails;
            private set { _selectedClientDetails = value; OnPropertyChanged(); }
        }

        // История заказов для DataGrid
        public ObservableCollection<Order> ClientOrdersHistory { get; }

        public ICommand SaveChangesCommand { get; }
        public ICommand CloseDetailsCommand { get; }
        #endregion

        #region Связь
        private ClientListItemViewModel _selectedClientInList;
        public ClientListItemViewModel SelectedClientInList
        {
            get => _selectedClientInList;
            set
            {
                if (_selectedClientInList == value) return;

                if (ShouldCancelSelectionChange(value))
                {
                    OnPropertyChanged(nameof(SelectedClientInList));
                    return;
                }

                if (_selectedClientInList != null) _selectedClientInList.IsSelected = false;
                _selectedClientInList = value;
                if (_selectedClientInList != null) _selectedClientInList.IsSelected = true;

                OnPropertyChanged();
                LoadClientDetailsAsync(value?.ClientId);
            }
        }
        #endregion

        public ClientsViewModel(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;

            FilteredClients = new ObservableCollection<ClientListItemViewModel>();
            ClientOrdersHistory = new ObservableCollection<Order>();

            SelectClientCommand = new RelayCommand<ClientListItemViewModel>(vm => SelectedClientInList = vm);
            AddClientCommand = new RelayCommand(ExecuteAddClient);
            SaveChangesCommand = new RelayCommand(ExecuteSaveChanges, () => SelectedClientDetails != null);
            CloseDetailsCommand = new RelayCommand(() => SelectedClientInList = null);
        }

        public async Task RefreshAsync()
        {
            if (_context.ChangeTracker.HasChanges()) _context.ChangeTracker.Clear();
            SearchText = string.Empty;
            SelectedClientInList = null;
            await LoadAllClientsAsync();
        }

        public async Task LoadInitialDataAsync()
        {
            await LoadAllClientsAsync();
        }

        private async Task LoadAllClientsAsync()
        {
            // Загружаем клиентов и сразу считаем количество заказов
            var clients = await _context.Clients
                .AsNoTracking()
                .Include(c => c.Orders) // Нужно для подсчета
                .OrderBy(c => c.SurName)
                .ToListAsync();

            _allClients = clients.Select(c => new ClientListItemViewModel(c)).ToList();
            ApplyFilters();
        }

        private async void LoadClientDetailsAsync(int? clientId)
        {
            if (clientId == null)
            {
                SelectedClientDetails = null;
                ClientOrdersHistory.Clear();
                return;
            }

            // Загружаем клиента для редактирования
            SelectedClientDetails = await _context.Clients
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Status) // Для истории
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Device) // Для истории
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (SelectedClientDetails != null)
            {
                ClientOrdersHistory.Clear();
                // Заполняем историю заказов (сортируем от новых к старым)
                foreach (var order in SelectedClientDetails.Orders.OrderByDescending(o => o.RegistrationDate))
                {
                    ClientOrdersHistory.Add(order);
                }
            }
        }

        private async void ExecuteAddClient()
        {
            var window = (AddClientWindow)_serviceProvider.GetService(typeof(AddClientWindow));
            if (window.ShowDialog() == true)
            {
                await LoadAllClientsAsync();
            }
        }

        private bool ShouldCancelSelectionChange(ClientListItemViewModel target)
        {
            if (!_context.ChangeTracker.HasChanges())
            {
                _context.ChangeTracker.Clear();
                return false;
            }

            var result = MessageBox.Show("Есть несохраненные изменения. Сохранить?", "Внимание", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                SaveInternalAsync(); // Синхронный запуск, но с ожиданием внутри нельзя.
                // Упрощение: сохраняем и переходим.
                return false;
            }
            else if (result == MessageBoxResult.No)
            {
                _context.ChangeTracker.Clear();
                return false;
            }
            return true; // Cancel
        }

        private async void ExecuteSaveChanges()
        {
            await SaveInternalAsync();
        }

        private async Task<bool> SaveInternalAsync()
        {
            try
            {
                if (SelectedClientDetails == null) return false;
                await _context.SaveChangesAsync();

                if (SelectedClientInList != null)
                {
                    SelectedClientInList.RefreshData(SelectedClientDetails);
                }

                MessageBox.Show("Сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                SelectedClientInList = null; // Закрываем панель
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ApplyFilters()
        {
            var result = _allClients.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lower = SearchText.ToLower();
                result = result.Where(c =>
                    c.FullName.ToLower().Contains(lower) ||
                    c.PhoneNumber.Contains(lower) ||
                    c.Email.Contains(lower));
            }

            FilteredClients.Clear();
            foreach (var item in result) FilteredClients.Add(item);
        }
    }
}