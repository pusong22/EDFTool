using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Kernel.Painting;
using EdfViewerApp.Chart;
using EdfViewerApp.Chart.Drawing;
using EdfViewerApp.Store;
using System.Collections.ObjectModel;

namespace EdfViewerApp.ViewModel;
public partial class ChartViewModel : BaseViewModel,
    IRecipient<ValueChangedMessage<IEnumerable<SignalViewModel>>>
{
    private readonly EDFStore _edfStore;
    [ObservableProperty]
    private ObservableCollection<Axis> _xAxes = [];

    [ObservableProperty]
    private ObservableCollection<Axis> _yAxes = [];

    [ObservableProperty]
    private ObservableCollection<LineSeriesProxy> _series = [];

    [ObservableProperty]
    private LabelVisual _title = new()
    {
        Text = "Test",
        TextPaint = new Brush() { FontSize = 18 },
    };


    [ObservableProperty]
    private double _timeMinimum = 0d;

    [ObservableProperty]
    private double _timeMaximum = 10d;

    [ObservableProperty]
    private double _currentTime;
    private List<SignalViewModel> _currentSelectedSignals = [];

    public ChartViewModel(EDFStore edfStore)
    {
        WeakReferenceMessenger.Default.Register(this);

        _edfStore = edfStore;
        _edfStore.InitializeTimeRangeHandler += InitializeTimeRange;
    }

    private void InitializeTimeRange(object s, EventArgs e)
    {
        double totalDuration = _edfStore.GetTotalDurationInSeconds(); // Example method
        TimeMinimum = 0; // EDF typically starts at time 0
        TimeMaximum = totalDuration;

        // Adjust the slider's initial position to the beginning
        CurrentTime = TimeMinimum; // This will trigger OnCurrentTimeChanged and load initial data
    }

    partial void OnCurrentTimeChanged(double value)
    {
        if (value >= TimeMaximum) return;

        for (int i = 0; i < _currentSelectedSignals.Count; i++)
        {
            var buf = _edfStore.ReadPhysicalData(_currentSelectedSignals[i].Id, (int)value, 10);

            var series = Series[i];
            series.XOffset = value;
            series.Data = [.. buf];
        }
    }

    public void Receive(ValueChangedMessage<IEnumerable<SignalViewModel>> message)
    {
        _currentSelectedSignals = [.. message.Value]; // Store selected signals for later use

        XAxes.Clear();
        XAxes.Add(new Axis()
        {
            Name = "Time",
            NamePaint = new Brush(),
            AxisLinePaint = new Pen(),
            SeparatorPaint = new Pen(new DashEffectSetting([3, 3])),
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
                SeparatorPaint = new Pen(new DashEffectSetting([3, 3])),
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
