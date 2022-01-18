using Apollo.BL;
using Apollo.Terminal.Services;
using Apollo.Terminal.Utils;
using Apollo.Terminal.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;

namespace Apollo.Terminal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();

            var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false);

            Configuration = builder.Build();

            ConfigureServices(services);
            this.ServiceProvider = services.BuildServiceProvider();

            EventManager.RegisterClassHandler(typeof(Window), Window.MouseUpEvent, new RoutedEventHandler(Window_MouseUp));

            var mainWindow = this.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = this.ServiceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void Window_MouseUp(object sender, RoutedEventArgs e)
        {
            this.ServiceProvider.GetService<ResetTimer>().Reset();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            BlFactory.RegisterServices(services, this.Configuration)
                    .AddSingleton<ViewManager>()

                    .AddSingleton<MainWindow>()
                    .AddSingleton<MainViewModel>()

                    .AddSingleton<MovieSelectionViewModel>()
                    .AddSingleton<ReservationViewModel>()
                    .AddSingleton<CinemaHallViewModel>()
                    .AddSingleton<CheckoutViewModel>()
                    .AddSingleton<PurchaseSuccessfullViewModel>()

                    .AddSingleton<IDialogService, TerminalDialogService>()
                    .AddSingleton<PrintService>()
                    .AddSingleton<SpinnerService>()
                    .AddSingleton<ResetTimer>()
                    .AddSingleton<IConfiguration>(c => this.Configuration)
                    .AddLogging(configure => configure.AddLog4Net());
        }
    }
}
