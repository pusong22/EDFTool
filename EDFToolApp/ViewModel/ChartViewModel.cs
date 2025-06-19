using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Core.Helper;
using Core.Kernel.Axis;
using Core.Kernel.Painting;
using Core.Kernel.Series;
using EDFToolApp.Chart.Drawing;
using System.Collections.ObjectModel;

namespace EDFToolApp.ViewModel;
public partial class ChartViewModel : BaseViewModel,
    IRecipient<ValueChangedMessage<IEnumerable<SignalViewModel>>>
{
    [ObservableProperty]
    private ObservableCollection<CoreAxis> _xAxes = [];

    [ObservableProperty]
    private ObservableCollection<CoreAxis> _yAxes = [];

    [ObservableProperty]
    private ObservableCollection<CoreLineSeries> _series = [];

    public ChartViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
        ChartConfig.DisabledAnimation = true;


        XAxes.Add(new Axis()
        {
            Name = "Time",
            AxisLinePaint = new Pen(),
            SeparatorPaint = new Pen(new DashEffectSetting([3, 3])),
            TickPaint = new Pen(),
            Labeler = l => l.ToString("N2")
        });

    }

    public void Receive(ValueChangedMessage<IEnumerable<SignalViewModel>> message)
    {
        YAxes.Clear();

        foreach (var item in message.Value)
        {
            YAxes.Add(new Axis()
            {
                Name = item.Label,
                AxisLinePaint = new Pen(),
                SeparatorPaint = new Pen(new DashEffectSetting([3, 3])),
                TickPaint = new Pen(),
                Labeler = l => l.ToString("N2"),
            });
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        WeakReferenceMessenger.Default.Unregister<ValueChangedMessage<IEnumerable<SignalViewModel>>>(this);
    }
}
