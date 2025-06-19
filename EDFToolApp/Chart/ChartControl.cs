using Core.Helper;
using Core.Kernel.Axis;
using Core.Kernel.Chart;
using Core.Kernel.Series;
using Core.Kernel.View;
using Core.Kernel.Visual;
using System.Windows;
using System.Windows.Controls;

namespace EDFToolApp.Chart;
public abstract class ChartControl : UserControl, IChartView
{
    private readonly CollectionWatcher<IEnumerable<CoreSeries>> _seriesWatcher;
    private readonly CollectionWatcher<IEnumerable<CoreAxis>> _xAxesWatcher;
    private readonly CollectionWatcher<IEnumerable<CoreAxis>> _yAxesWatcher;

    protected ChartControl()
    {
        Content = CanvasControl;

        ChartConfig.Configure(config => config.UseDefault());

        _seriesWatcher = new CollectionWatcher<IEnumerable<CoreSeries>>(() => CoreChart?.Update());
        _xAxesWatcher = new CollectionWatcher<IEnumerable<CoreAxis>>(() => CoreChart?.Update());
        _yAxesWatcher = new CollectionWatcher<IEnumerable<CoreAxis>>(() => CoreChart?.Update());

        Loaded += OnLoad;
        Unloaded += OnUnLoad;
        SizeChanged += OnSizeChanged;
    }

    #region DP
    public VisualElement Title
    {
        get { return (VisualElement)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public IEnumerable<CoreSeries> Series
    {
        get { return (IEnumerable<CoreSeries>)GetValue(SeriesProperty); }
        set { SetValue(SeriesProperty, value); }
    }

    public IEnumerable<CoreAxis> XAxes
    {
        get { return (IEnumerable<CoreAxis>)GetValue(XAxesProperty); }
        set { SetValue(XAxesProperty, value); }
    }

    public IEnumerable<CoreAxis> YAxes
    {
        get { return (IEnumerable<CoreAxis>)GetValue(YAxesProperty); }
        set { SetValue(YAxesProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            "Title",
            typeof(VisualElement),
            typeof(ChartControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SeriesProperty =
        DependencyProperty.Register(
            "Series",
            typeof(IEnumerable<CoreSeries>),
            typeof(ChartControl),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is ChartControl chartControl)
                {
                    chartControl._seriesWatcher.WatchedCollection = (IEnumerable<CoreSeries>)e.NewValue;
                }
            }));



    public static readonly DependencyProperty XAxesProperty =
        DependencyProperty.Register(
            "XAxes",
            typeof(IEnumerable<CoreAxis>),
            typeof(ChartControl),
            new PropertyMetadata(null, (d, e) =>
            {
                if (d is ChartControl chartControl)
                {
                    chartControl._xAxesWatcher.WatchedCollection = (IEnumerable<CoreAxis>)e.NewValue;
                }

            }));

    public static readonly DependencyProperty YAxesProperty =
       DependencyProperty.Register(
           "YAxes",
           typeof(IEnumerable<CoreAxis>),
           typeof(ChartControl),
              new PropertyMetadata(null, (d, e) =>
              {
                  if (d is ChartControl chartControl)
                  {
                      chartControl._yAxesWatcher.WatchedCollection = (IEnumerable<CoreAxis>)e.NewValue;
                  }

              }));

    #endregion

    protected CanvasControl CanvasControl { get; } = new();
    protected abstract CoreChart? CoreChart { get; }

    public Core.Primitive.Size ControlSize => new((float)CanvasControl.ActualWidth, (float)CanvasControl.ActualHeight);

    public CanvasContext CanvasContext => CanvasControl.CanvasContext;


    public void InvokeUIThread(Action action)
    {
        _ = Dispatcher.BeginInvoke(action);
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        CoreChart?.Load();
    }

    private void OnUnLoad(object sender, RoutedEventArgs e)
    {
        CoreChart?.UnLoad();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        CoreChart?.Update();
    }
}
