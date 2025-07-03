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
using SkiaSharp;
using System.Collections.ObjectModel;

namespace EdfViewerApp.ViewModel;
public partial class HeatSeriesViewModel(EDFStore edfStore) : BaseViewModel
{
    private static Color[] _default = [
        new Color(0, 0, 0, 255),      // 黑色 (低亮度)
        new Color(255, 255, 255, 255) // 白色 (高亮度)
    ];

    private static Color[] _viridisMap = [
        new Color(68, 1, 84),
        new Color(71, 44, 122),
        new Color(59, 81, 139),
        new Color(44, 113, 142),
        new Color(33, 144, 141),
        new Color(39, 173, 129),
        new Color(92, 200, 99),
        new Color(170, 220, 50),
        new Color(253, 231, 37)
    ];

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
    private ObservableCollection<HeatSeries> _series = [];

    [ObservableProperty]
    private ObservableCollection<SignalViewModel> _channels = [];

    [ObservableProperty]
    private SignalViewModel? _selectedChannel;

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

        double[] buf = edfStore.ReadPhysicalData(value.Id);

        //double[] filterCoefficients = FirCoefficients.BandPass(signal1.SampleRate, 0.5, 50.0, 0);
        //OnlineFirFilter filter = new(filterCoefficients);
        //var filtered = new double[buf1.Length];
        //for (int i = 0; i < buf1.Length; i++)
        //{
        //    filtered[i] = filter.ProcessSample(buf1[i]);
        //}

        int sampleEpoch = 512;
        int overlap = 256;
        var result = EegProcessor.ComputeSpectrogram(
            buf,
            value.SampleRate,
            sampleEpoch,
            overlap);
        var data = new List<Coordinate>();

        for (int r = 0; r < result.SpectrogramData!.GetLength(0); r++) // 频率 (Y轴)
        {
            double currentFrequencyHz = result.FrequenciesHz![r];

            //可以选择在这里进行频率范围的筛选，例如只保留0Hz到31Hz的数据
            if (currentFrequencyHz < 0 || currentFrequencyHz > 31.0)
                break; // 跳过超出范围的频率

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

        Series.Clear();
        Series.Add(new HeatSeries()
        {
            Values = [.. data],
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
        });
    }

    private static IEnumerable<Coordinate> LoadMonaLisaPoints(
        string imagePath, int targetWidth, int targetHeight)
    {
        // 加载图片
        var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
        bitmapImage.DecodePixelWidth = targetWidth; // 缩放宽度
        bitmapImage.DecodePixelHeight = targetHeight; // 缩放高度
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        // 转成 FormatConvertedBitmap 确保格式为 Bgra32 (每像素4字节)
        var formattedBitmap = new System.Windows.Media.Imaging.FormatConvertedBitmap(bitmapImage, System.Windows.Media.PixelFormats.Bgra32, null, 0);
        formattedBitmap.Freeze();

        int width = formattedBitmap.PixelWidth;
        int height = formattedBitmap.PixelHeight;
        int stride = width * 4; // 每行字节数

        var pixels = new byte[height * stride];
        formattedBitmap.CopyPixels(pixels, stride, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * stride + x * 4;
                byte b = pixels[idx + 0];
                byte g = pixels[idx + 1];
                byte r = pixels[idx + 2];
                // byte a = pixels[idx + 3]; // 如果需要 alpha

                // 计算亮度（灰度），和你原来权重一样
                double z = (r * 0.3 + g * 0.59 + b * 0.11) / 255.0;

                yield return new Coordinate(x, y, z);
            }
        }
    }
}
