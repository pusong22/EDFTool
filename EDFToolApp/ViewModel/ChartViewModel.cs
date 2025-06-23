using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Kernel.Painting;
using EDFToolApp.Chart;
using EDFToolApp.Chart.Drawing;
using EDFToolApp.Store;
using System.Collections.ObjectModel;

namespace EDFToolApp.ViewModel;
public partial class ChartViewModel : BaseViewModel,
    IRecipient<ValueChangedMessage<IEnumerable<SignalViewModel>>>
{
    private readonly EDFStore _edfStore;
    [ObservableProperty]
    private ObservableCollection<Axis> _xAxes = [];

    [ObservableProperty]
    private ObservableCollection<Axis> _yAxes = [];

    [ObservableProperty]
    private ObservableCollection<LineSeries<double>> _series = [];

    [ObservableProperty]
    private LabelVisual _title = new()
    {
        Text = "Test",
        TextPaint = new Brush() { FontSize = 18 },
    };


    public ChartViewModel(EDFStore edfStore)
    {
        WeakReferenceMessenger.Default.Register(this);

        _edfStore = edfStore;
    }

    public void Receive(ValueChangedMessage<IEnumerable<SignalViewModel>> message)
    {
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
        foreach (var item in message.Value)
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

        foreach (var item in message.Value)
        {
            // TEST data
            var buf = _edfStore.ReadPhysicalData(item.Id, 0, 1);
            Series.Add(new LineSeries<double>(buf) { YIndex = item.Id });
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        WeakReferenceMessenger.Default.Unregister<ValueChangedMessage<IEnumerable<SignalViewModel>>>(this);
    }
}
