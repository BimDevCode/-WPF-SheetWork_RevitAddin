using SheetWork.Core;
using SheetWork.EventHandler;
using SheetWork.Services;
using SheetWork.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui.Contracts;
using Wpf.Ui.Services;

namespace SheetWork;
/// <summary>
/// Class define all DI container
/// They start work with revit to decrease time to open
/// </summary>
public static class Host
{
    private static IHost _host;

    public static Task StartHost()
    {
        _host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Page resolver service
                services.AddSingleton<IPageService, PageService>();
                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddTransient<INavigationWindow, MainWindow>();
                services.AddTransient<ViewModels.MainWindowViewModel>();
                services.AddSingleton<ISnackbarService, SnackbarService>();

                // Views and ViewModels
                services.AddTransient<Views.Pages.SheetTablePage>();
                services.AddTransient<ViewModels.SheetTableViewModel>();

                services.AddTransient<CopySheetAsyncEvent>();
                services.AddTransient<DeleteSheetAsyncEvent>();
                services.AddTransient<RenameSheetAsyncEvent>();
                services.AddTransient<ExportAsync>();

            }).Build();

        _host.Start();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stop DI Container on revit terminate
    /// </summary>
    public static async Task StopHost()
    {
        await _host.StopAsync();
        _host.Dispose();
    }

    /// <summary>
    /// Get needed DI container
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }
}