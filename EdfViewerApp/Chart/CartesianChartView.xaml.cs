using Core.Helper;
using Core.Kernel;
using Core.Kernel.Visual;
using EdfViewerApp.Chart.Drawing;
using SkiaSharp.Views.Desktop;
using System.Windows;
using System.Windows.Controls;

namespace EdfViewerApp.Chart;
/// <summary>
/// Interaction logic for CartesianChartView.xaml
/// </summary>
public partial class CartesianChartView : UserControl, ICartesianChartView
{
    private readonly CollectionWatcher<IEnumerable<ILineSeries>> _seriesWatcher;
    private readonly CollectionWatcher<IEnumerable<ICartesianAxis>> _xAxesWatcher;
    private readonly CollectionWatcher<IEnumerable<ICartesianAxis>> _yAxesWatcher;

    private readonly CartesianChart _cartesianChart;

    public CartesianChartView()
    {
        InitializeComponent();

        _cartesianChart = new CartesianChart(this);
        _cartesianChart.RedrawHandler += OnRedrawHandler;

        ChartConfig.Configure(config => config.UseDefault());

        _seriesWatcher = new CollectionWatcher<IEnumerable<ILineSeries>>(() => _cartesianChart?.Update());
        _xAxesWatcher = new CollectionWatcher<IEnumerable<ICartesianAxis>>(() => _cartesianChart?.Update());
        _yAxesWatcher = new CollectionWatcher<IEnumerable<ICartesianAxis>>(() => _cartesianChart?.Update());

        Loaded += OnLoad;
        Unloaded += OnUnLoad;
        SizeChanged += OnSizeChanged;
    }

    #region DP
    public IBaseLabelVisual Title
    {
        get { return (IBaseLabelVisual)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public IEnumerable<ILineSeries> Series
    {
        get { return (IEnumerable<ILineSeries>)GetValue(SeriesProperty); }
        set { SetValue(SeriesProperty, value); }
    }

    public IEnumerable<ICartesianAxis> XAxes
    {
        get { return (IEnumerable<ICartesianAxis>)GetValue(XAxesProperty); }
        set { SetValue(XAxesProperty, value); }
    }

    public IEnumerable<ICartesianAxis> YAxes
    {
        get { return (IEnumerable<ICartesianAxis>)GetValue(YAxesProperty); }
        set { SetValue(YAxesProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            "Title",
            typeof(IBaseLabelVisual),
            typeof(CartesianChartView),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is CartesianChartView o)
                {
                    o._cartesianChart?.Update();
                }
            }));

    public static readonly DependencyProperty SeriesProperty =
        DependencyProperty.Register(
            "Series",
            typeof(IEnumerable<ILineSeries>),
            typeof(CartesianChartView),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is CartesianChartView o)
                {
                    o._seriesWatcher.WatchedCollection = (IEnumerable<ILineSeries>)e.NewValue;
                }
            }));



    public static readonly DependencyProperty XAxesProperty =
        DependencyProperty.Register(
            "XAxes",
            typeof(IEnumerable<ICartesianAxis>),
            typeof(CartesianChartView),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is CartesianChartView o)
                {
                    o._xAxesWatcher.WatchedCollection = (IEnumerable<ICartesianAxis>)e.NewValue;
                }

            }));

    public static readonly DependencyProperty YAxesProperty =
       DependencyProperty.Register(
           "YAxes",
           typeof(IEnumerable<ICartesianAxis>),
           typeof(CartesianChartView),
              new PropertyMetadata(null, (d, e) =>
              {
                  if (d is CartesianChartView o)
                  {
                      o._yAxesWatcher.WatchedCollection = (IEnumerable<ICartesianAxis>)e.NewValue;
                  }

              }));

    #endregion

    public Core.Primitive.Size ControlSize => new((float)ActualWidth, (float)ActualHeight);

    public void InvokeUIThread(Action action)
    {
        Dispatcher.BeginInvoke(action);
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        _cartesianChart?.Load();
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {
        _cartesianChart?.UnLoad();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _cartesianChart?.Update();
    }

    private void OnRedrawHandler(object sender, EventArgs e)
    {
        _skElement.InvalidateVisual();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var context = new SkiaSharpDrawnContext(e.Surface, e.Info);
        _cartesianChart?.DrawFrame(context);
    }
}
