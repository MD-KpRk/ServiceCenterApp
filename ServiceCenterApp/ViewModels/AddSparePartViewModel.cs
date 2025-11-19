using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using System.Collections.Generic;
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

        private readonly ObservableCollection<SparePart> _allSpareParts;
        public ICollectionView SparePartsView { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
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

            SparePartsView = CollectionViewSource.GetDefaultView(_allSpareParts);
            SparePartsView.Filter = FilterSpareParts;
        }

        public async Task LoadSparePartsAsync(Dictionary<int, int> existingQuantities)
        {
            var parts = await _context.SpareParts
                .AsNoTracking()
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();

            _allSpareParts.Clear();
            foreach (var part in parts)
            {
                if (existingQuantities.ContainsKey(part.PartId))
                {
                    part.StockQuantity -= existingQuantities[part.PartId];
                }

                if (part.StockQuantity > 0)
                {
                    _allSpareParts.Add(part);
                }
            }
        }

        private bool FilterSpareParts(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            if (item is SparePart part)
            {
                return part.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                       part.PartNumber.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}