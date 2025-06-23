using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

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

            UnsubscribeFromCollection(_watchedCollection);
            _watchedCollection = value;
            SubscribeFromCollection(_watchedCollection);

            _updateAction?.Invoke();
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

    private void UnsubscribeFromCollection(TCollection? collection)
    {
        if (_watchedCollection is INotifyCollectionChanged notifier)
            notifier.CollectionChanged -= OnInternalCollectionChanged;

        if (collection is not null)
        {
            foreach (var item in collection)
                UnsubscribeFromItem(item);
        }
    }


    private void SubscribeFromCollection(TCollection? collection)
    {
        if (_watchedCollection is INotifyCollectionChanged notifier)
            notifier.CollectionChanged += OnInternalCollectionChanged;

        if (collection is not null)
        {
            foreach (var item in collection)
                SubscribeFromItem(item);
        }
    }


    private void UnsubscribeFromItem(object item)
    {
        if (item is INotifyPropertyChanged inpc)
            inpc.PropertyChanged -= OnItemPropertyChanged;
    }


    private void SubscribeFromItem(object item)
    {
        if (item is INotifyPropertyChanged inpc)
            inpc.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnInternalCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var oldItem in e.OldItems)
                UnsubscribeFromItem(oldItem);
        }

        if (e.NewItems is not null)
        {
            foreach (var newItem in e.NewItems)
                SubscribeFromItem(newItem);
        }

        _updateAction?.Invoke();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
            UnsubscribeFromCollection(_watchedCollection);

            _watchedCollection = default;
            _updateAction = null!;
        }
    }
}
