using CommunityToolkit.Mvvm.ComponentModel;
using Core.Kernel.Axis;
using Core.Kernel.Painting;
using Core.Kernel.Series;
using SkiaSharpBackend;
using SkiaSharpBackend.Drawing;

namespace EDFToolApp.ViewModel;
public partial class ChartViewModel : BaseViewModel
{
    [ObservableProperty]
    private CoreAxis[] _xAxes = [
        new Axis() {
            Name = "Time",
            AxisLinePaint = new Pen(),
            SeparatorPaint = new Pen(new DashEffectSetting([3,3])),
            TickPaint = new Pen(),
            Labeler=l=>l.ToString("N2"),
        }
    ];

    [ObservableProperty]
    private CoreAxis[] _yAxes = [
        new Axis() {
            Name = "Magnitude",
            AxisLinePaint = new Pen(),
            SeparatorPaint = new Pen(new DashEffectSetting([3,3])),
            TickPaint = new Pen(),
            Labeler=l=>l.ToString("N2"),
        }
    ];

    [ObservableProperty]
    private CoreLineSeries[] _series = [
        new LineSeries<double>([.. Fetch()])
        {
            LineSmoothness = 0.85f,
            VisualGeometrySize = 20f,
            SampleInterval = 1/10d,
        }
    ];

    private static IEnumerable<double> Fetch()
    {
        for (double x = 0; x < 100; x += 0.1)
        {
            yield return Math.Sin(x);
        }
    }
}
