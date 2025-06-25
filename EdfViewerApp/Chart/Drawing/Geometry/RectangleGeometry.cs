using Core.Kernel.Drawing.Geometry;
using SkiaSharp;

namespace EdfViewerApp.Chart.Drawing.Geometry;
public class RectangleGeometry : BaseRectangleGeometry
{
    public override void Draw<TDrawnContext>(TDrawnContext context)
    {
        context.DrawRect(new SKRect(X, Y, X + Width, Y + Height));
    }
}
