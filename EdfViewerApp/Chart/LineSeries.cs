using Core.Kernel;
using EdfViewerApp.Chart.Drawing.Geometry;

namespace EdfViewerApp.Chart;
public class LineSeries<TValueType> : CoreLineSeries<TValueType, CubicBezierVectorGeometry>
{
    public LineSeries() { }
}
