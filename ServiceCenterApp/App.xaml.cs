using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Pages;
using ServiceCenterApp.Services;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace ServiceCenterApp
{
    public partial class App : Application
    {
        private readonly ServiceProvider? _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            // --- SERVICE REGISTRATION ---

            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // --- VIEWMODELS ---
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<AuthPageViewModel>();

            // VIEWS
            services.AddTransient<AuthPage>(); // Page
            services.AddSingleton<MainWindow>(s => new MainWindow(s.GetRequiredService<INavigationService>(), s.GetRequiredService<ICurrentUserService>()) // Window
            {
                DataContext = s.GetRequiredService<MainWindowViewModel>()
            });


            _serviceProvider = services.BuildServiceProvider();
            ConfigureNavigation();
        }

        private void ConfigureNavigation()
        {
            NavigationService? navService = _serviceProvider?.GetRequiredService<INavigationService>() as NavigationService;

            if (navService == null) return;

            navService?.Configure<AuthPageViewModel, AuthPage>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (_serviceProvider == null) throw new ArgumentNullException();
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
