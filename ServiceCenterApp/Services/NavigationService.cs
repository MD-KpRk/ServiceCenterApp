using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Attributes;
using ServiceCenterApp.Data;
using ServiceCenterApp.Data.Configurations;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly ApplicationDbContext? _dbcontext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDatabaseHealthService _databaseHealthService;
        private Frame? _mainFrame;

        public NavigationService(
            ApplicationDbContext? dbcontext, 
            IServiceProvider serviceProvider,
            IDatabaseHealthService databaseHealthService,
            ICurrentUserService currentUserService
            )
        {
            _databaseHealthService = databaseHealthService;
            _serviceProvider = serviceProvider;
            _currentUserService = currentUserService;
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

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            if (_mainFrame == null)
                throw new InvalidOperationException("NavigationService is not initialized.");

            if (!_viewModelToViewMapping.ContainsKey(typeof(TViewModel)))
                throw new KeyNotFoundException($"No page configured for ViewModel {typeof(TViewModel).Name}");

            Type pageType = _viewModelToViewMapping[typeof(TViewModel)];
            RequiredPermissionAttribute? requiredPermissionAttribute = pageType.GetCustomAttribute<RequiredPermissionAttribute>();

            if (requiredPermissionAttribute != null)
            {
                PermissionEnum[] requiredPermissions = requiredPermissionAttribute.RequiredPermissions;
                if (!_currentUserService.HasAllPermissions(requiredPermissions))
                {
                    MessageBox.Show("У вас нет прав для доступа к этому разделу.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            BaseViewModel? viewModel = _serviceProvider.GetService(typeof(TViewModel)) as BaseViewModel;
            Page? page = _serviceProvider.GetService(pageType) as Page;

            if (page == null || viewModel == null)
            {
                throw new NullReferenceException($"Could not resolve page or ViewModel from DI container for {typeof(TViewModel).Name}");
            }

            page.DataContext = viewModel;
            _mainFrame.Navigate(page);
        }

        public void NavigaToRoleMainPage()
        {
            if(_currentUserService.HasAllPermissions(PermissionEnum.Admin))
                NavigateTo<MainAdminPageViewModel>();
            if (_currentUserService.HasAllPermissions(PermissionEnum.Orders))
                NavigateTo<OrdersPageViewModel>();

        }

        public void StartNavigation()
        {
            if (_dbcontext == null)
                throw new ArgumentNullException(nameof(_dbcontext));

            if(_dbcontext.Employees.Where(e => e.RoleId == ((int)RoleEnum.Administrator)).Count() == 0)
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