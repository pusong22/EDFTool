using System.Collections;
using System.Collections.Specialized;

namespace EdfViewerApp;
public class CollectionWatcher<TCollection>(Action updateAction) : IDisposable
    where TCollection : IEnumerable
{
    private TCollection? _watchedCollection;
    private Action _updateAction = updateAction
        ?? throw new ArgumentNullException(nameof(updateAction));

    public TCollection? WatchedCollection
    {
        get => _watchedCollection;
        set
        {
            if (Equals(_watchedCollection, value)) return;

            if (_watchedCollection is INotifyCollectionChanged oldNotifier)
            {
                oldNotifier.CollectionChanged -= OnInternalCollectionChanged;
            }

            _watchedCollection = value;

            if (_watchedCollection is INotifyCollectionChanged newNotifier)
            {
                newNotifier.CollectionChanged += OnInternalCollectionChanged;
            }

            _updateAction?.Invoke();
        }
    }

    private void OnInternalCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _updateAction?.Invoke();
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_watchedCollection is INotifyCollectionChanged notifier)
            {
                notifier.CollectionChanged -= OnInternalCollectionChanged;
            }

            _watchedCollection = default;
            _updateAction = null!;
        }
    }
}
