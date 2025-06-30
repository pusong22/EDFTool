using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Kernel.Painting;
using Core.Primitive;
using EdfViewerApp.Chart;
using EdfViewerApp.Chart.Drawing;
using EdfViewerApp.Eeg;
using EdfViewerApp.Store;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace EdfViewerApp.ViewModel;
public partial class LineSeriesViewModel : BaseViewModel,
    IRecipient<ValueChangedMessage<IEnumerable<SignalViewModel>>>
{
    private readonly EDFStore _edfStore;
    private CancellationTokenSource? _debouncingCts;

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

    public LineSeriesViewModel(EDFStore edfStore)
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

    private void LoadDataAt(double time)
    {
        for (int i = 0; i < _currentSelectedSignals.Count; i++)
        {
            var signal = _currentSelectedSignals[i];

            var buf = _edfStore.ReadPhysicalData(signal.Id, (int)time, 10);

            var series = Series[i];
            series.XOffset = time;
            series.Data = [.. buf];
        }

        var signal1 = _currentSelectedSignals[0];
        var buf1 = _edfStore.ReadPhysicalData(signal1.Id, (int)time, 10);

        int sampleEpoch = (int)(signal1.SampleRate * 1.0d);
        int overlap = (int)(sampleEpoch * 0);
        var result = EegProcessor.ComputeSpectrogram(
            buf1,
            signal1.SampleRate,
            sampleEpoch,
            overlap,
            0.5,
            50.0);
        var data = new List<Coordinate>();

        for (int r = 0; r < result.SpectrogramData!.GetLength(0); r++) // 频率 (Y轴)
        {
            double currentFrequencyHz = result.FrequenciesHz![r];

            // 可以选择在这里进行频率范围的筛选，例如只保留0Hz到31Hz的数据
            //if (currentFrequencyHz < 0 || currentFrequencyHz > 31.0)
            //    continue; // 跳过超出范围的频率

            for (int c = 0; c < result.SpectrogramData!.GetLength(1); c++) // 时间 (X轴)
            {
                double power = result.SpectrogramData[r, c];
                // 过滤掉 MinValue (log(0) 产生的)
                if (power > double.MinValue)
                {
                    double currentTimeSeconds = result.TimesSeconds![c];
                    // X=时间索引, Y=频率索引, Weight=功率
                    var coord = new Coordinate(currentTimeSeconds, currentFrequencyHz, power);
                    data.Add(coord);
                }
            }
        }

        WeakReferenceMessenger.Default.Send(data);
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
