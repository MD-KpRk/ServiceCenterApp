using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    // ДОБАВЛЕН ИНТЕРФЕЙС IRefreshable
    public class EmployeesViewModel : BaseViewModel, IRefreshable
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(1, 1);

        #region Список (Left Panel)
        private List<EmployeeListItemViewModel> _allEmployees = new List<EmployeeListItemViewModel>();
        public ObservableCollection<EmployeeListItemViewModel> FilteredEmployees { get; }
        public ICommand SelectEmployeeCommand { get; }
        public ICommand AddEmployeeCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }
        #endregion

        #region Детали (Right Panel)
        private Employee _selectedEmployeeDetails;
        public Employee SelectedEmployeeDetails
        {
            get => _selectedEmployeeDetails;
            private set { _selectedEmployeeDetails = value; OnPropertyChanged(); }
        }

        public List<Role> AllRoles { get; private set; }

        private ObservableCollection<Position> _allPositions;
        public ObservableCollection<Position> AllPositions
        {
            get => _allPositions;
            private set { _allPositions = value; OnPropertyChanged(); }
        }

        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                if (SelectedEmployeeDetails != null && value != null)
                    SelectedEmployeeDetails.RoleId = value.RoleId;
            }
        }

        private Position _selectedPosition;
        public Position SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                _selectedPosition = value;
                OnPropertyChanged();
                if (SelectedEmployeeDetails != null && value != null)
                    SelectedEmployeeDetails.PositionId = value.PositionId;
            }
        }

        private int _completedOrdersCount;
        public int CompletedOrdersCount { get => _completedOrdersCount; set { _completedOrdersCount = value; OnPropertyChanged(); } }

        public ICommand SaveChangesCommand { get; }
        public ICommand CloseDetailsCommand { get; }
        public ICommand AddPositionCommand { get; }
        #endregion

        #region Связь
        private EmployeeListItemViewModel _selectedEmployeeInList;
        public EmployeeListItemViewModel SelectedEmployeeInList
        {
            get => _selectedEmployeeInList;
            set
            {
                if (_selectedEmployeeInList == value) return;

                if (ShouldCancelSelectionChange(value))
                {
                    OnPropertyChanged(nameof(SelectedEmployeeInList));
                    return;
                }

                if (_selectedEmployeeInList != null) _selectedEmployeeInList.IsSelected = false;
                _selectedEmployeeInList = value;
                if (_selectedEmployeeInList != null) _selectedEmployeeInList.IsSelected = true;

                OnPropertyChanged();

                LoadEmployeeDetailsAsync(value?.EmployeeId);
            }
        }
        #endregion

        public EmployeesViewModel(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            FilteredEmployees = new ObservableCollection<EmployeeListItemViewModel>();

            SelectEmployeeCommand = new RelayCommand<EmployeeListItemViewModel>(vm => SelectedEmployeeInList = vm);
            AddEmployeeCommand = new RelayCommand(ExecuteAddEmployee);
            SaveChangesCommand = new RelayCommand(ExecuteSaveChanges, () => SelectedEmployeeDetails != null);
            CloseDetailsCommand = new RelayCommand(CloseDetails);
            AddPositionCommand = new RelayCommand(ExecuteAddPosition);
        }

        // --- РЕАЛИЗАЦИЯ IRefreshable ---
        public async Task RefreshAsync()
        {
            // 1. Сбрасываем изменения в контексте, чтобы не было конфликтов при перезагрузке
            if (_context.ChangeTracker.HasChanges())
            {
                _context.ChangeTracker.Clear();
            }

            // 2. Очищаем поиск
            SearchText = string.Empty;

            // 3. Закрываем панель деталей (снимаем выделение)
            // Работаем напрямую с полем и уведомлением, чтобы обойти логику проверки сохранения
            if (_selectedEmployeeInList != null) _selectedEmployeeInList.IsSelected = false;
            _selectedEmployeeInList = null;
            OnPropertyChanged(nameof(SelectedEmployeeInList));

            // Очищаем объект деталей
            SelectedEmployeeDetails = null;

            // 4. Перезагружаем данные из БД
            await LoadInitialDataAsync();
        }
        // --------------------------------

        private void CloseDetails()
        {
            SelectedEmployeeInList = null;
        }

        private async void ExecuteAddPosition()
        {
            var window = new AddPositionWindow();
            if (window.ShowDialog() == true)
            {
                string newName = window.PositionName;

                if (AllPositions.Any(p => p.PositionName.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Такая должность уже существует.");
                    SelectedPosition = AllPositions.First(p => p.PositionName.Equals(newName, StringComparison.OrdinalIgnoreCase));
                    return;
                }

                await _dbSemaphore.WaitAsync();
                try
                {
                    var newPos = new Position { PositionName = newName };
                    _context.Positions.Add(newPos);
                    await _context.SaveChangesAsync();

                    AllPositions.Add(newPos);
                    SelectedPosition = newPos;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при создании должности: " + ex.Message);
                }
                finally
                {
                    _dbSemaphore.Release();
                }
            }
        }

        public async Task LoadInitialDataAsync()
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                AllRoles = await _context.Roles.ToListAsync();
                var positions = await _context.Positions.ToListAsync();
                AllPositions = new ObservableCollection<Position>(positions);

                OnPropertyChanged(nameof(AllRoles));
                OnPropertyChanged(nameof(AllPositions));

                await LoadAllEmployeesInternalAsync();
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async Task LoadAllEmployeesInternalAsync()
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Role)
                .Include(e => e.Position)
                .OrderBy(e => e.SurName)
                .ToListAsync();

            _allEmployees = employees.Select(e => new EmployeeListItemViewModel(e)).ToList();

            Application.Current.Dispatcher.Invoke(() => ApplyFilters());
        }

        private async Task LoadAllEmployeesAsync()
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                await LoadAllEmployeesInternalAsync();
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async void LoadEmployeeDetailsAsync(int? empId)
        {
            if (empId == null)
            {
                SelectedEmployeeDetails = null;
                return;
            }

            await _dbSemaphore.WaitAsync();
            try
            {
                SelectedEmployeeDetails = await _context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Position)
                    .Include(e => e.AcceptedOrders).ThenInclude(o => o.Status)
                    .FirstOrDefaultAsync(e => e.EmployeeId == empId);

                if (SelectedEmployeeDetails != null)
                {
                    SelectedRole = AllRoles.FirstOrDefault(r => r.RoleId == SelectedEmployeeDetails.RoleId);
                    SelectedPosition = AllPositions.FirstOrDefault(p => p.PositionId == SelectedEmployeeDetails.PositionId);
                    CompletedOrdersCount = SelectedEmployeeDetails.AcceptedOrders.Count(o => o.Status.StatusName == "Выдан");
                }
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private async void ExecuteAddEmployee()
        {
            var window = (AddEmployeeWindow)_serviceProvider.GetService(typeof(AddEmployeeWindow));
            if (window.ShowDialog() == true)
            {
                await LoadAllEmployeesAsync();
            }
        }

        private bool ShouldCancelSelectionChange(EmployeeListItemViewModel target)
        {
            if (!_context.ChangeTracker.HasChanges())
            {
                _context.ChangeTracker.Clear();
                return false;
            }

            var res = MessageBox.Show("У вас есть несохраненные изменения. Сохранить их перед переходом?",
                                      "Несохраненные изменения",
                                      MessageBoxButton.YesNoCancel,
                                      MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                SaveAndNavigateAsync(target);
                return true;
            }
            else if (res == MessageBoxResult.No)
            {
                _context.ChangeTracker.Clear();
                return false;
            }

            return true;
        }

        private async void SaveAndNavigateAsync(EmployeeListItemViewModel target)
        {
            if (await SaveInternalAsync())
            {
                _context.ChangeTracker.Clear();
                SelectedEmployeeInList = target;
            }
        }

        private async void ExecuteSaveChanges()
        {
            if (await SaveInternalAsync())
            {
                SelectedEmployeeInList = null;
            }
        }

        private async Task<bool> SaveInternalAsync()
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                if (SelectedEmployeeDetails == null) return false;

                await _context.SaveChangesAsync();

                // Очистка пустых должностей
                var unusedPositions = await _context.Positions
                    .Include(p => p.Employees)
                    .Where(p => !p.Employees.Any())
                    .ToListAsync();

                if (unusedPositions.Any())
                {
                    _context.Positions.RemoveRange(unusedPositions);
                    await _context.SaveChangesAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var pos in unusedPositions)
                        {
                            var itemToRemove = AllPositions.FirstOrDefault(p => p.PositionId == pos.PositionId);
                            if (itemToRemove != null) AllPositions.Remove(itemToRemove);
                        }
                    });
                }

                if (SelectedEmployeeInList != null)
                {
                    if (SelectedEmployeeDetails.RoleId != 0)
                        SelectedEmployeeDetails.Role = await _context.Roles.FindAsync(SelectedEmployeeDetails.RoleId);

                    if (SelectedEmployeeDetails.PositionId != 0)
                        SelectedEmployeeDetails.Position = await _context.Positions.FindAsync(SelectedEmployeeDetails.PositionId);

                    SelectedEmployeeInList.RefreshData(SelectedEmployeeDetails);
                }

                MessageBox.Show("Сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }

        private void ApplyFilters()
        {
            var result = _allEmployees.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string lower = SearchText.ToLower();
                result = result.Where(e => e.FullName.ToLower().Contains(lower));
            }
            FilteredEmployees.Clear();
            foreach (var item in result) FilteredEmployees.Add(item);
        }
    }
}