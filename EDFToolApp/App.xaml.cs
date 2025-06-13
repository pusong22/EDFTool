using EDFToolApp.EFDbContext;
using EDFToolApp.HostBuilder;
using Microsoft.EntityFrameworkCore;
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
            .AddDbContext()
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


#if DEBUG
        IDbContextFactory factory = provider.GetRequiredService<IDbContextFactory>();
        using (FileDbContext context = factory.CreateDbContext())
        {
            context.Database.Migrate();
        }
#endif
        //var router = provider.GetRequiredService<ViewModelRouter>();
        //router.NavigateTo(RouterName.FileView);

        Current.MainWindow = provider.GetRequiredService<MainWindow>();

        bool? dialogResult = provider.GetRequiredService<StartupWindow>().ShowDialog();
        if (dialogResult == true)
        {
            Current.MainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        _host.StopAsync();

        _host.Dispose();
    }
}

