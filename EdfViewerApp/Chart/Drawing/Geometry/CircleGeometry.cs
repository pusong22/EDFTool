using Core.Kernel.Drawing.Geometry;
using SkiaSharp;

namespace EdfViewerApp.Chart.Drawing.Geometry;
public class CircleGeometry : BaseRectangleGeometry
{
    public override void Draw<TDrawnContext>(TDrawnContext context)
    {
        float radius = Width / 2f;
        context.DrawCircle(new SKPoint(X, Y), radius);
        context.DrawCircle(new SKPoint(X, Y), radius / 9);
    }
}
