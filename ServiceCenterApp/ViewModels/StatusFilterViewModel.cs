using System;

namespace ServiceCenterApp.ViewModels
{
    public class StatusFilterViewModel : BaseViewModel
    {
        private readonly Action _onFilterChanged;
        private bool _isChecked;

        public string StatusName { get; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                    _onFilterChanged?.Invoke();
                }
            }
        }

        public StatusFilterViewModel(string statusName, Action onFilterChanged)
        {
            StatusName = statusName;
            _onFilterChanged = onFilterChanged;
            _isChecked = true; 
        }
    }
}