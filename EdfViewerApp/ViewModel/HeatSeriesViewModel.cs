using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Kernel.Painting;
using Core.Primitive;
using EdfViewerApp.Chart;
using EdfViewerApp.Chart.Drawing;
using EdfViewerApp.Eeg;
using EdfViewerApp.Store;
using MathNet.Filtering.FIR;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Markup;

namespace EdfViewerApp.ViewModel;
public partial class HeatSeriesViewModel(EDFStore edfStore) : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Axis> _xAxes = [
        new Axis()
            {
                Name = "Time",
                NamePaint = new Brush(),
                LabelPaint = new Brush(),
                Labeler = l => l.ToString("N2")
            }];

    [ObservableProperty]
    private ObservableCollection<Axis> _yAxes = [
        new Axis()
            {
                Name = "Frequent",
                NamePaint = new Brush(),
                LabelPaint = new Brush(),
                Labeler = l => l.ToString("N2")
            }];

    [ObservableProperty]
    private ObservableCollection<HeatSeriesProxy> _series = [
        new HeatSeriesProxy()
        {
            HeatMap = [
                    new Color(0, 0, 128),    // 深蓝色 - 非常低
                    new Color(0, 128, 255),  // 淡蓝 - 低
                    new Color(0, 255, 255),  // 青色 - 稍低
                    new Color(0, 255, 0),    // 绿色 - 正常值附近
                    new Color(255, 255, 0),  // 黄色 - 偏高
                    new Color(255, 128, 0),  // 橙色 - 高
                    new Color(255, 0, 0)     // 红色 - 非常高
                ],
            HeatPaint = new Brush(),
        }];

    [ObservableProperty]
    private ObservableCollection<SignalViewModel> _channels = [];

    [ObservableProperty]
    private SignalViewModel? _selectedChannel;

    private Stopwatch sb = new ();

    [RelayCommand]
    private void LoadChannels()
    {
        if (!edfStore.Open) return;

        Channels = new(edfStore.SignalVMs);
        SelectedChannel = Channels.FirstOrDefault();
    }

    partial void OnSelectedChannelChanged(SignalViewModel? value)
    {
        if (value is null) return;
        List<Coordinate> data = new();

        sb.Restart();
        // 耗时
        double[] buf = edfStore.ReadPhysicalData(value.Id);
        // 耗时!!!
        //double[] filterCoefficients = FirCoefficients.BandPass(value.SampleRate, 0.5, 50.0, 0);
        //OnlineFirFilter filter = new(filterCoefficients);
        //var filtered = new double[buf.Length];
        //for (int i = 0; i < buf.Length; i++)
        //{
        //    filtered[i] = filter.ProcessSample(buf[i]);
        //}

        // 耗时
        (double[] freqs, double[] times, double[,] spectrogram) =
            WelchPSD.ComputeSpectrogram(
            buf,
            fs: 500,
            nperseg: 500,
            noverlap: 250);

        sb.Stop();
        Debug.WriteLine($"{sb.ElapsedMilliseconds} ms");

        for (int r = 0; r < spectrogram.GetLength(0); r++)
        {
            double currentTimeSeconds = times[r];

            for (int c = 0; c < spectrogram.GetLength(1); c++)
            {
                double currentFrequencyHz = freqs[c];

                // 耗时ui卡顿
                if (currentFrequencyHz < 0 || currentFrequencyHz > 31.0)
                    continue;

                double power = spectrogram[r, c];

                if (power > double.MinValue)
                    data.Add(new(currentTimeSeconds, currentFrequencyHz, power));
            }
        }

        Series[0].Data = [.. data];
    }
}
