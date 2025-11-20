using ServiceCenterApp.Models;
using System.Linq;
using System.Windows.Media;

namespace ServiceCenterApp.ViewModels
{
    public class EmployeeListItemViewModel : BaseViewModel
    {
        public int EmployeeId { get; }

        private string _fullName;
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

        private string _initials;
        public string Initials { get => _initials; set { _initials = value; OnPropertyChanged(); } }

        private string _positionName;
        public string PositionName { get => _positionName; set { _positionName = value; OnPropertyChanged(); } }

        private string _roleName;
        public string RoleName { get => _roleName; set { _roleName = value; OnPropertyChanged(); } }

        private Brush _roleColor;
        public Brush RoleColor { get => _roleColor; set { _roleColor = value; OnPropertyChanged(); } }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        public EmployeeListItemViewModel(Employee employee)
        {
            EmployeeId = employee.EmployeeId;
            RefreshData(employee);
        }

        public void RefreshData(Employee employee)
        {
            // ФИО
            string s = employee.SurName?.Trim() ?? "";
            string n = employee.FirstName?.Trim() ?? "";
            string p = employee.Patronymic?.Trim() ?? "";
            FullName = $"{s} {n} {p}".Trim();

            // Должность и Роль
            PositionName = employee.Position?.PositionName ?? "—";
            RoleName = employee.Role?.RoleName ?? "—";

            // --- БЕЗОПАСНЫЕ ИНИЦИАЛЫ (Как у клиентов) ---
            string firstLetter = !string.IsNullOrEmpty(s) ? s.Substring(0, 1) : "";
            string secondLetter = !string.IsNullOrEmpty(n) ? n.Substring(0, 1) : "";
            string res = (firstLetter + secondLetter).ToUpper();
            Initials = string.IsNullOrEmpty(res) ? "?" : res;

            // Цвет бейджа в зависимости от ID роли (Hardcoded mapping for visuals)
            // 1: Admin (Red), 2: Reception (Blue), 3: Master (Green)
            if (employee.Role != null)
            {
                switch (employee.Role.RoleId)
                {
                    case 1: RoleColor = new SolidColorBrush(Color.FromRgb(220, 53, 69)); break; // Red
                    case 2: RoleColor = new SolidColorBrush(Color.FromRgb(13, 110, 253)); break; // Blue
                    case 3: RoleColor = new SolidColorBrush(Color.FromRgb(25, 135, 84)); break;  // Green
                    default: RoleColor = new SolidColorBrush(Colors.Gray); break;
                }
            }
            else
            {
                RoleColor = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}