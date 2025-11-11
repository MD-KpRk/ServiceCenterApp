using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ServiceCenterApp.Services
{
    public class NavigationService : INavigationService
    {
        // ViewModel -> View
        private readonly Dictionary<Type, Type> _viewModelToViewMapping = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _dbcontext;
        private Frame? _mainFrame;

        public NavigationService(ApplicationDbContext dbcontext, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dbcontext = dbcontext;
        }

        public void Initialize(Frame frame)
        {
            _mainFrame = frame;
        }

        public void GoBack()
        {
            if (_mainFrame == null) throw new ArgumentNullException(nameof(_mainFrame));

            if (_mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            if (_mainFrame == null) throw new ArgumentNullException(nameof(_mainFrame));

            Type viewModelType = typeof(TViewModel);

            if (!_viewModelToViewMapping.TryGetValue(viewModelType, out var viewType))
            {
                throw new InvalidOperationException($"Нет сопоставления для ViewModel: {viewModelType.Name}");
            }

            var page = _serviceProvider.GetRequiredService(viewType) as Page;

            if (page == null)
            {
                throw new InvalidOperationException($"Не удалось создать страницу для ViewModel: {viewModelType.Name}");
            }

            _mainFrame.Navigate(page);
        }

        public void StartNavigation()
        {
            if(_dbcontext.Employees == null)
            {
#warning USE CUSTOM ERROR
                MessageBox.Show("Users table not found ");
                return;
            }
            if(_dbcontext.Employees.Count() == 0)
            {
                NavigateTo<InstallationPageViewModel>();
            }
            else
            {
                NavigateTo<AuthPageViewModel>();
            }
        }

        public void Configure<TViewModel, TView>() where TView : Page
        {
            _viewModelToViewMapping.Add(typeof(TViewModel), typeof(TView));
        }
    }
}