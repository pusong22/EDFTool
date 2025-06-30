using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Kernel.Painting;
using Core.Primitive;
using EdfViewerApp.Chart;
using EdfViewerApp.Chart.Drawing;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace EdfViewerApp.ViewModel;
public partial class HeatSeriesViewModel : BaseViewModel, IRecipient<List<Coordinate>>
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

    public HeatSeriesViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

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

    public void Receive(List<Coordinate> message)
    {
        Series.Clear();
        Series.Add(new HeatSeries()
        {
            Values = [.. message],
            HeatMap = [
                  new Color(0, 0, 255), // 蓝色 (低功率)
                  new Color(0, 255, 0), // 绿色 (中等功率)
                  new Color(255, 0, 0), // 红色 (高功率)
                ],
            HeatPaint = new Brush(),
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        WeakReferenceMessenger.Default.Unregister<IEnumerable<Coordinate>>(this);
    }
}
