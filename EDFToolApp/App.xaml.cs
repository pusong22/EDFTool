using EDFToolApp.HostBuilder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace EDFToolApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .AddStores()
            .AddServices()
            .AddViewModels()
            .AddViews()
            .Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host.Start();

        var provider = _host.Services;

        Current.MainWindow = provider.GetRequiredService<MainWindow>();
        Current.MainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        await _host.StopAsync();

        _host.Dispose();
    }
}

