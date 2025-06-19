using Core.Helper;
using EDFToolApp.Chart.Drawing;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System.Windows;
using System.Windows.Controls;

namespace EDFToolApp.Chart;
public class CanvasControl : UserControl
{
    private bool _invalidating = false;
    private SKElement? _skElement;
    //private SKGLElement? _skGLElement;

    public CanvasControl()
    {
        InitializeSkElement();

        CanvasContext.InvalidatedHandler += OnInvalidate;

        Unloaded += OnUnLoad;
    }

    public Core.Kernel.Chart.CanvasContext CanvasContext { get; } = new();

    private void InitializeSkElement()
    {
        if (ChartConfig.USE_GPU)
        {
            //Content = _skGLElement = new SKGLElement();
            //_skGLElement.PaintSurface += OnPaintGLSurface;
        }
        else
        {
            Content = _skElement = new SKElement();
            _skElement.PaintSurface += OnPaintSurface;
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (!_invalidating) return;
        var context = new SkiaSharpDrawnContext(e.Surface, e.Info);
        CanvasContext.DrawFrame(context);
    }

    private void OnPaintGLSurface(object sender, SKPaintGLSurfaceEventArgs e)
    {
        if (!_invalidating) return;
        var context = new SkiaSharpDrawnContext(e.Surface, e.Info);
        CanvasContext.DrawFrame(context);
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {
        CanvasContext.InvalidatedHandler -= OnInvalidate;
    }

    private void OnInvalidate(object sender, EventArgs e)
    {
        Loop();
    }

    private async void Loop()
    {
        if (_invalidating) return; // 丢弃一些绘制
        _invalidating = true;

        var ts = TimeSpan.FromSeconds(1 / ChartConfig.MaxFPS);

        while (!CanvasContext.IsCompleted)
        {
            _skElement?.InvalidateVisual();
            //_skGLElement?.InvalidateVisual();

            await Task.Delay(ts);
        }

        _invalidating = false;
    }
}
