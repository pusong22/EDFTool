using Core.Kernel.Chart;
using Core.Kernel.View;
using Core.Primitive;

namespace EDFToolApp.Chart;
public class CartesianChartControl : ChartControl, ICartesianChartView
{
    private CoreChart? _coreChart;

    public LayoutKind LayoutKind { get; set; } = LayoutKind.Stack;

    protected override CoreChart? CoreChart
    {
        get
        {
            _coreChart ??= new CartesianChart(this);
            return _coreChart;
        }
    }
}
