using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Pages;
using ServiceCenterApp.Services;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ServiceCenterApp
{
    public partial class App : Application
    {
        private readonly ServiceProvider? _serviceProvider;
        public static IConfiguration? Configuration { get; private set; }

        public App()
        {

            // CONFIG

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // DI CONTAINER INIT

            IServiceCollection services = new ServiceCollection();

            // DATABASE REGISTATION

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // --- SERVICE REGISTRATION ---

            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // --- VIEWMODELS ---
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<AuthPageViewModel>();

            // VIEWS
            services.AddTransient<AuthPage>(); // Page
            services.AddSingleton<MainWindow>(s => new MainWindow() // Window
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


            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
