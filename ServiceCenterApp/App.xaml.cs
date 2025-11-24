using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceCenterApp.Data;
using ServiceCenterApp.Models.Config;
using ServiceCenterApp.Pages;
using ServiceCenterApp.Services;
using ServiceCenterApp.Services.Interfaces;
using ServiceCenterApp.ViewModels;
using ServiceCenterApp.Views;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ServiceCenterApp
{
    public partial class App : Application
    {
        private readonly ServiceProvider? _serviceProvider;
        public static IConfiguration? Configuration { get; private set; }

        private bool _isExiting = false;

        public App()
        {
            // Глобальная обработка ошибок
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // CONFIG
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // --- DI CONTAINER INIT ---
            IServiceCollection services = new ServiceCollection();

            services.Configure<OrganizationSettings>(settings =>
            {
                IConfigurationSection section = Configuration.GetSection("OrganizationSettings");

                // Вручную перекладываем значения из конфига в класс
                settings.Name = section["Name"];
                settings.Address = section["Address"];
                settings.UNP = section["UNP"];
                settings.Phone = section["Phone"];
            });


            // --- DATABASE REGISTRATION ---
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            }, ServiceLifetime.Transient); // Transient важно для WPF, чтобы избегать проблем с потоками


            // --- SERVICE REGISTRATION ---
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPasswordHasher, PasswordHashService>();
            services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
            services.AddSingleton<IPrintService, PrintService>();
            services.AddTransient<IDocumentService, DocumentService>();

            // --- VIEWMODELS ---
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<InstallationPageViewModel>();
            services.AddTransient<AuthPageViewModel>();
            services.AddTransient<MainAdminPageViewModel>();
            services.AddTransient<OrdersViewModel>();
            services.AddTransient<AddSparePartViewModel>();
            services.AddTransient<AddOrderViewModel>();
            services.AddTransient<ClientsViewModel>();
            services.AddTransient<AddClientViewModel>();
            services.AddTransient<EmployeesViewModel>();
            services.AddTransient<AddEmployeeViewModel>();
            services.AddTransient<StorageViewModel>();
            services.AddTransient<AddEditSparePartViewModel>();
            services.AddTransient<AddServiceViewModel>();
            services.AddTransient<AddEditServiceViewModel>();
            services.AddTransient<FinanceViewModel>();
            services.AddTransient<AddTransactionViewModel>();

            // --- VIEWS ---
            services.AddSingleton<MainWindow>();                // Window
            services.AddTransient<AuthPage>();                  // Page
            services.AddTransient<InstallationPage>();          // Page
            services.AddTransient<MainAdminPage>();             // Page 
            services.AddTransient<OrdersPage>();                // Page 
            services.AddTransient<AddSparePartWindow>();        // Window
            services.AddTransient<AddOrderWindow>();            // Window
            services.AddTransient<ClientsPage>();               // Page 
            services.AddTransient<AddClientWindow>();           // Window
            services.AddTransient<EmployeesPage>();             // Page
            services.AddTransient<AddEmployeeWindow>();         // Window
            services.AddTransient<StoragePage>();               // Page
            services.AddTransient<AddEditSparePartWindow>();    // Window
            services.AddTransient<AddServiceWindow>();          // Window
            services.AddTransient<AddEditServiceWindow>();      // Window
            services.AddTransient<FinancePage>();               // Page
            services.AddTransient<AddTransactionWindow>();      // Window

            _serviceProvider = services.BuildServiceProvider();
            ConfigureNavigation();
        }

        private void ConfigureNavigation()
        {
            if (_serviceProvider?.GetRequiredService<INavigationService>() is not NavigationService navService) return;

            // VIEWMODEL - PAGE MAPPING
            navService.Configure<AuthPageViewModel, AuthPage>();
            navService.Configure<InstallationPageViewModel, InstallationPage>();
            navService.Configure<MainAdminPageViewModel, MainAdminPage>();
            navService.Configure<OrdersViewModel, OrdersPage>();
            navService.Configure<ClientsViewModel, ClientsPage>();
            navService.Configure<EmployeesViewModel, EmployeesPage>();
            navService.Configure<StorageViewModel, StoragePage>();
            // === НОВАЯ НАВИГАЦИЯ ===
            navService.Configure<FinanceViewModel, FinancePage>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (_serviceProvider == null) throw new ArgumentNullException(nameof(_serviceProvider));

            // --- УСТАНОВКА КУЛЬТУРЫ ДЛЯ ОТОБРАЖЕНИЯ ВАЛЮТЫ (BYN) ---
            CultureInfo culture = new CultureInfo("be-BY");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Миграция и проверка БД
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var healthService = scope.ServiceProvider.GetRequiredService<IDatabaseHealthService>();

                try
                {
                    // Пытаемся применить миграции автоматически
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении базы данных (Migration): {ex.Message}\n\n" +
                                    "Проверьте настройки подключения.",
                                    "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Проверка соединения
                bool isConnected = await healthService.CanConnectAsync();
                if (!isConnected)
                {
                    MessageBox.Show("Не удалось установить соединение с SQL Server.\n" +
                                    "Пожалуйста, проверьте строку подключения в appsettings.json и доступность сервера.",
                                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }

            // Show MainWindow
            try
            {
                MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации главного окна: {ex.Message}");
                Shutdown();
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _isExiting = true;
            if (_serviceProvider != null)
            {
                _serviceProvider.Dispose();
            }

            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_isExiting)
            {
                e.Handled = true;
                return;
            }

            if (e.Exception is TaskCanceledException)
            {
                e.Handled = true;
                return;
            }

            string errorMsg = $"Произошла непредвиденная ошибка: {e.Exception.Message}\n\n";
            if (e.Exception.InnerException != null)
            {
                errorMsg += $"Подробности: {e.Exception.InnerException.Message}";
            }

            MessageBox.Show(errorMsg, "Ошибка приложения", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}