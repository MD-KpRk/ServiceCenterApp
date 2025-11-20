using ServiceCenterApp.Models;
using System.Linq;

namespace ServiceCenterApp.ViewModels
{
    public class ClientListItemViewModel : BaseViewModel
    {
        public int ClientId { get; }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _initials;
        public string Initials
        {
            get => _initials;
            set { _initials = value; OnPropertyChanged(); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private int _ordersCount;
        public int OrdersCount
        {
            get => _ordersCount;
            set { _ordersCount = value; OnPropertyChanged(); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public ClientListItemViewModel(Client client)
        {
            ClientId = client.ClientId;
            RefreshData(client);
        }

        public void RefreshData(Client client)
        {
            string s = client.SurName?.Trim() ?? "";
            string n = client.FirstName?.Trim() ?? "";
            string p = client.Patronymic?.Trim() ?? "";

            FullName = $"{s} {n} {p}".Trim();

            string firstLetter = "";
            string secondLetter = "";

            if (!string.IsNullOrEmpty(s))
            {
                firstLetter = s.Substring(0, 1);
            }

            if (!string.IsNullOrEmpty(n))
            {
                secondLetter = n.Substring(0, 1);
            }

            string result = (firstLetter + secondLetter).ToUpper();

            if (string.IsNullOrEmpty(result))
            {
                result = "?";
            }

            Initials = result;

            PhoneNumber = client.PhoneNumber;
            Email = string.IsNullOrEmpty(client.Email) ? "—" : client.Email;
            OrdersCount = client.Orders?.Count ?? 0;
        }
    }
}