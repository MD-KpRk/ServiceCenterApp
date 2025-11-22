using Microsoft.EntityFrameworkCore;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenterApp.ViewModels
{
    public class AddServiceViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _context;

        public List<Service> AllServices { get; private set; }
        public Service SelectedService { get; set; }

        public AddServiceViewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LoadServicesAsync()
        {
            AllServices = await _context.Services.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
            OnPropertyChanged(nameof(AllServices));
        }
    }
}