using Core.Kernel;
using EdfViewerApp.Chart.Drawing.Geometry;

namespace EdfViewerApp.Chart;
public class LineSeries<TValueType>
    : CoreLineSeries<TValueType, CircleGeometry, CubicBezierVectorGeometry>
{
    public LineSeries() : base(null) { }

    public LineSeries(IReadOnlyCollection<TValueType> values) : base(values) { }
}
