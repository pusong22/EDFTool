using Core.Kernel.Drawing.Geometry;
using SkiaSharp;

namespace EdfViewerApp.Chart.Drawing.Geometry;

public class LineGeometry : BaseLineGeometry
{
    public override void Draw<TDrawnContext>(TDrawnContext context)
    {
        context.DrawLine(new SKPoint(X, Y), new SKPoint(X1, Y1));
    }
}
