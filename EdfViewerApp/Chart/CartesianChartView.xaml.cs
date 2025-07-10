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
    private readonly CollectionWatcher<IEnumerable<ICartesianSeries>> _seriesWatcher;
    private readonly CollectionWatcher<IEnumerable<ICartesianAxis>> _xAxesWatcher;
    private readonly CollectionWatcher<IEnumerable<ICartesianAxis>> _yAxesWatcher;

    private readonly CartesianChart _cartesianChart;
    private ChartDrawingCommand? _latestDrawCommand;
    private int _updateLock = 0;
    private bool _hasPendingReDraw = false; // 是否执行过redraw


    public CartesianChartView()
    {
        InitializeComponent();

        _cartesianChart = new CartesianChart(this);

        ChartConfig.Configure(config => config.UseDefault());

        _seriesWatcher = new CollectionWatcher<IEnumerable<ICartesianSeries>>(ReDraw);
        _xAxesWatcher = new CollectionWatcher<IEnumerable<ICartesianAxis>>(ReDraw);
        _yAxesWatcher = new CollectionWatcher<IEnumerable<ICartesianAxis>>(ReDraw);

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

    public IEnumerable<ICartesianSeries> Series
    {
        get { return (IEnumerable<ICartesianSeries>)GetValue(SeriesProperty); }
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
                    o.ReDraw();
                }
            }));

    public static readonly DependencyProperty SeriesProperty =
        DependencyProperty.Register(
            "Series",
            typeof(IEnumerable<ICartesianSeries>),
            typeof(CartesianChartView),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is CartesianChartView o)
                {
                    o._seriesWatcher.WatchedCollection = (IEnumerable<ICartesianSeries>)e.NewValue;
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

    public void BeginUpdate()
    {
        Interlocked.Increment(ref _updateLock);
    }

    public void EndUpdate()
    {
        if (Interlocked.Decrement(ref _updateLock) == 0 && _hasPendingReDraw)
        {
            _hasPendingReDraw = false;
            ReDraw();
        }
    }

    public void ReDraw()
    {
        if (_updateLock > 0)
        {
            _hasPendingReDraw = true;
            return;
        }

        if (Dispatcher.CheckAccess())
        {
            ChatModelSnapshot snapshot = new()
            {
                ControlSize = new Core.Primitive.Size((float)_skElement.ActualWidth, (float)_skElement.ActualHeight),
                Title = Title,
                XAxes = XAxes,
                YAxes = YAxes,
                Series = Series,
            };

            _cartesianChart?.UpdateAsync(snapshot);
        }
        else
            Dispatcher.BeginInvoke(() => ReDraw());
    }

    public void RequestInvalidateVisual(ChartDrawingCommand command)
    {
        if (Dispatcher.CheckAccess())
        {
            _latestDrawCommand = command;

            _skElement?.InvalidateVisual();
        }
        else
            Dispatcher.BeginInvoke(() => RequestInvalidateVisual(command));
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        _cartesianChart?.Load();
        ReDraw();
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {
        _cartesianChart?.UnLoad();
        _latestDrawCommand = null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ReDraw();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        if (!IsLoaded || _latestDrawCommand is null) return;

        SkiaSharpDrawnContext context = new(e.Surface, e.Info);

        ChartDrawingCommand commandToExecute = _latestDrawCommand;

        commandToExecute.Execute(context);
    }
}
