using CommunityToolkit.Mvvm.ComponentModel;

namespace EDFToolApp.ViewModel;
public abstract class BaseViewModel : ObservableObject, IDisposable
{
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            disposedValue = true;
        }
    }

    ~BaseViewModel()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
