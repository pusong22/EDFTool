using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Kernel.Painting;
using EdfViewerApp.Chart;
using EdfViewerApp.Chart.Drawing;
using EdfViewerApp.Store;
using System.Collections.ObjectModel;

namespace EdfViewerApp.ViewModel;
public partial class LineSeriesViewModel : BaseViewModel,
    IRecipient<ValueChangedMessage<IEnumerable<SignalViewModel>>>
{
    private const int _step = 10;
    private bool _resetThumb = false;
    private readonly EDFStore _edfStore;
    private CancellationTokenSource? _debouncingCts;
    private List<SignalViewModel> _currentSelectedSignals = [];

    [ObservableProperty]
    private ObservableCollection<Axis> _xAxes = [];

    [ObservableProperty]
    private ObservableCollection<Axis> _yAxes = [];

    [ObservableProperty]
    private ObservableCollection<LineSeriesProxy> _series = [];

    [ObservableProperty]
    private double _timeMinimum = 0d;

    [ObservableProperty]
    private double _timeMaximum = 10d;

    [ObservableProperty]
    private double _currentTime;

    public LineSeriesViewModel(EDFStore edfStore)
    {
        WeakReferenceMessenger.Default.Register(this);

        _edfStore = edfStore;
    }

    private void ResetThumb()
    {
        _resetThumb = true;
        double totalDuration = _edfStore.GetTotalDurationInSeconds();
        TimeMinimum = 0;
        TimeMaximum = totalDuration / _step;

        CurrentTime = 0;
        _resetThumb = false;
    }

    partial void OnCurrentTimeChanged(double value)
    {
        if (_resetThumb) return;

        if (value >= TimeMaximum) return;

        // debouncing
        _debouncingCts?.Cancel();
        _debouncingCts = new CancellationTokenSource();
        var token = _debouncingCts.Token;

        Task.Delay(300).ContinueWith(t =>
        {
            if (token.IsCancellationRequested) return;

            LoadDataAt(value);

        }, token);
    }

    private async void LoadDataAt(double time)
    {
        double actualTime = time * _step;
        if (actualTime >= TimeMaximum)
            actualTime -= _step;

        for (int i = 0; i < _currentSelectedSignals.Count; i++)
        {
            var signal = _currentSelectedSignals[i];

            var buf = await _edfStore.ReadPhysicalData(signal.Id, (int)actualTime, _step);

            var series = Series[i];
            series.XOffset = actualTime;
            series.Data = [.. buf];
        }
    }

    [RelayCommand]
    private void ThumbReleased()
    {
        // cancel
        _debouncingCts?.Cancel();

        // refresh
        LoadDataAt(CurrentTime);
    }

    public void Receive(ValueChangedMessage<IEnumerable<SignalViewModel>> message)
    {
        // Reset thumb
        ResetThumb();

        _currentSelectedSignals = [.. message.Value]; // Store selected signals for later use

        XAxes.Clear();
        XAxes.Add(new Axis()
        {
            LabelPaint = new Brush(),
            AxisLinePaint = new Pen(),
            Labeler = l => l.ToString("N2")
        });

        YAxes.Clear();
        foreach (var item in _currentSelectedSignals)
        {
            YAxes.Add(new Axis()
            {
                Name = item.Label,
                NamePaint = new Brush(),
                AxisLinePaint = new Pen(),
                Labeler = l => l.ToString("N2"),
            });
        }

        Series.Clear();

        int id = 0;
        foreach (var item in _currentSelectedSignals)
        {
            Series.Add(new LineSeriesProxy
            {
                YIndex = id++,
                SampleInterval = 1d / item.SampleRate,
            });
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        WeakReferenceMessenger.Default.Unregister<ValueChangedMessage<IEnumerable<SignalViewModel>>>(this);
    }
}
