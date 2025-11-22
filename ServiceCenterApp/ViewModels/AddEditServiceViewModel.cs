using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using ServiceCenterApp.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServiceCenterApp.ViewModels
{
    public class AddEditServiceViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        private bool _isEditMode;

        public string WindowTitle => _isEditMode ? "Редактирование услуги" : "Новая услуга";

        public int ServiceId { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }

        public ICommand SaveCommand { get; }

        public AddEditServiceViewModel(ApplicationDbContext context)
        {
            _context = context;
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        public void LoadData(Service serviceToEdit = null)
        {
            if (serviceToEdit != null)
            {
                _isEditMode = true;
                ServiceId = serviceToEdit.ServiceId;
                Name = serviceToEdit.Name;
                BasePrice = serviceToEdit.BasePrice;
            }
            else
            {
                _isEditMode = false;
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        private async void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Введите название услуги.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    var service = await _context.Services.FindAsync(ServiceId);
                    if (service != null)
                    {
                        service.Name = Name;
                        service.BasePrice = BasePrice;
                        _context.Services.Update(service);
                    }
                }
                else
                {
                    var newService = new Service
                    {
                        Name = Name,
                        BasePrice = BasePrice
                    };
                    _context.Services.Add(newService);
                }

                await _context.SaveChangesAsync();

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