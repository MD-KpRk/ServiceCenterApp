using Microsoft.EntityFrameworkCore;
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

namespace ServiceCenterApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<Type, Type> _viewModelToViewMapping = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext? _dbcontext;
        private readonly ICurrentUserService _currentUserService;
        private Frame? _mainFrame;

        public NavigationService(
            ApplicationDbContext? dbcontext,
            IServiceProvider serviceProvider,
            ICurrentUserService currentUserService)
        {
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
            if (_mainFrame != null && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        public void Configure<TViewModel, TPage>() where TViewModel : BaseViewModel where TPage : Page
        {
            _viewModelToViewMapping[typeof(TViewModel)] = typeof(TPage);
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel
        {
            if (_mainFrame == null)
                throw new InvalidOperationException("NavigationService is not initialized.");

            if (_mainFrame.Content is Page currentPage && currentPage.DataContext is TViewModel currentViewModel)
            {
                if (currentViewModel is IRefreshable refreshableViewModel)
                {
                    refreshableViewModel.RefreshAsync();
                    return;
                }
            }

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

            if (parameter != null && viewModel is IViewModelWithParameter vmWithParam)
            {
                vmWithParam.SetParameter(parameter);
            }

            page.DataContext = viewModel;
            _mainFrame.Navigate(page);
        }

        public void NavigateToRoleMainPage()
        {
            if (_currentUserService.HasAllPermissions(PermissionEnum.Admin))
            {
                NavigateTo<MainAdminPageViewModel>();
                return;
            }
            if (_currentUserService.HasAllPermissions(PermissionEnum.Orders))
            {
                NavigateTo<OrdersViewModel>();
                return;
            }
        }

        public void StartNavigation()
        {
            if (_dbcontext == null)
                throw new ArgumentNullException(nameof(_dbcontext));

            if (_dbcontext.Employees.Where(e => e.RoleId == ((int)RoleEnum.Administrator)).Count() == 0)
            {
                NavigateTo<InstallationPageViewModel>();
            }
            else
            {
                NavigateTo<AuthPageViewModel>();
            }
        }
    }
}