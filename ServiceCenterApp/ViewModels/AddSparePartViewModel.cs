using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ServiceCenterApp.ViewModels
{
    public class AddSparePartViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;

        // Коллекция для хранения всех запчастей
        private readonly ObservableCollection<SparePart> _allSpareParts;

        // "Представление" коллекции, которое мы будем фильтровать
        public ICollectionView SparePartsView { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Применяем фильтр к нашему "представлению"
                SparePartsView.Refresh();
            }
        }

        private SparePart _selectedSparePart;
        public SparePart SelectedSparePart
        {
            get => _selectedSparePart;
            set { _selectedSparePart = value; OnPropertyChanged(); }
        }

        public AddSparePartViewModel(ApplicationDbContext context)
        {
            _context = context;
            _allSpareParts = new ObservableCollection<SparePart>();

            // Создаем "представление" для фильтрации
            SparePartsView = CollectionViewSource.GetDefaultView(_allSpareParts);
            SparePartsView.Filter = FilterSpareParts;
        }

        public async Task LoadSparePartsAsync()
        {
            // Загружаем только те запчасти, которые есть в наличии
            var parts = await _context.SpareParts
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();

            _allSpareParts.Clear();
            foreach (var part in parts)
            {
                _allSpareParts.Add(part);
            }
        }

        private bool FilterSpareParts(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true; // Фильтр пуст - показываем всё
            }

            if (item is SparePart part)
            {
                // Поиск по названию или артикулу, без учета регистра
                return part.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                       part.PartNumber.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}