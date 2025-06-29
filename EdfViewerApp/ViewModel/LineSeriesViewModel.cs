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
        var data = GenerateSpectrogram(buf1, signal1.SampleRate, 10);

        WeakReferenceMessenger.Default.Send(data);
    }

    private IEnumerable<Coordinate> GenerateSpectrogram(double[] rawBuf, double sampleRate, int slices)
    {
        for (int i = 0; i < slices; i++)
        {
            // 1. 滤波
            // order如何指定？
            var filterBuf = EegProcessor.ApplyBandpassFilter(rawBuf, sampleRate, 1.0, 50.0, 10);
            // 2. （可选）分段
            // 3. 计算功率谱
            var powerSpectrum = EegProcessor.ComputePowerSpectrum(filterBuf);

            // 4. 计算频率对应的频点
            var freqBins = EegProcessor.GetFrequencyBins(filterBuf.Length, sampleRate);

            // 5. 计算频带功率
            //var bandPower = EegProcessor.CalculateBandPower(powerSpectrum, freqBins);

            // 5. 输出结果
            //Debug.WriteLine($"Delta={bandPower.Delta:F2}, Theta={bandPower.Theta:F2}, Alpha={bandPower.Alpha:F2}, Beta={bandPower.Beta:F2}, Gamma={bandPower.Gamma:F2}");

            for (int f = 0; f < freqBins.Length; f++)
            {
                double freqHz = freqBins[f];
                double power = powerSpectrum[f];

                yield return new Coordinate(i, freqHz, power);
            }
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
