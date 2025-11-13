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
using System.Globalization;
using System.IO;
using System.Windows;
using static System.Formats.Asn1.AsnWriter;

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

            // --- DI CONTAINER INIT ---

            IServiceCollection services = new ServiceCollection();

            // --- DATABASE REGISTATION ---
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // --- SERVICE REGISTRATION ---
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPasswordHasher, PasswordHashService>();
            services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();

            // --- VIEWMODELS ---
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<InstallationPageViewModel>();
            services.AddTransient<AuthPageViewModel>();
            services.AddTransient<MainAdminPageViewModel>();

            // --- VIEWS ---
            services.AddTransient<AuthPage>(); // Page
            services.AddSingleton<InstallationPage>(); // Page
            services.AddSingleton<MainWindow>(); // Window
            services.AddSingleton<MainAdminPage>(); // Page


            _serviceProvider = services.BuildServiceProvider();
            ConfigureNavigation();
        }

        private void ConfigureNavigation()
        {
            if (_serviceProvider?.GetRequiredService<INavigationService>() is not NavigationService navService) return;

            // VIEWMODEL - PAGE MAPPING
            navService?.Configure<AuthPageViewModel, AuthPage>();
            navService?.Configure<InstallationPageViewModel, InstallationPage>();
            navService?.Configure<MainAdminPageViewModel, MainAdminPage>();

        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (_serviceProvider == null) throw new ArgumentNullException();

            // --- УСТАНОВКА КУЛЬТУРЫ ДЛЯ ОТОБРАЖЕНИЯ ВАЛЮТЫ ---
            var culture = new CultureInfo("be-BY");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            using (var scope = _serviceProvider.CreateScope())
            {
                IDatabaseHealthService healthService = scope.ServiceProvider.GetRequiredService<IDatabaseHealthService>();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception)
                {

                }

                bool isConnected = await healthService.CanConnectAsync();
                if (!isConnected)
                {
                    MessageBox.Show("Не удалось установить соединение с SQL Server. " +
                                    "Пожалуйста, проверьте строку подключения в appsettings.json и доступность сервера.",
                                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

            }

            //Show MainWindow
            MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
