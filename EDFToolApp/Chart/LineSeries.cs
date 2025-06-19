using Core.Kernel.Series;
using EDFToolApp.Chart.Drawing.Geometry;

namespace EDFToolApp.Chart;
public class LineSeries<TValueType>
    : CoreLineSeries<TValueType, CircleGeometry, CubicBezierVectorGeometry>
{
    public LineSeries() : base(null) { }

    public LineSeries(IReadOnlyCollection<TValueType> values) : base(values) { }
}
