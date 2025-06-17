using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace EDFToolApp.Service;

public class NavigationWindowService(IServiceProvider provider)
{
    public void Show<TView>() where TView : UserControl
    {
        var view = provider.GetRequiredService<TView>();

        var window = provider.GetRequiredService<GenericWindow>();

        window.Content = view;

        window.Show();
    }

    public bool? ShowDialog<TView>() where TView : UserControl
    {
        var view = provider.GetRequiredService<TView>();

        var window = provider.GetRequiredService<GenericWindow>();

        window.Content = view;

        return window.ShowDialog();
    }
}
