using Core.Kernel;
using Core.Kernel.Drawing;
using Core.Kernel.Drawing.Geometry;
using SkiaSharp;

namespace EdfViewerApp.Chart.Drawing.Geometry;
public abstract class VectorGeometry<TSegment> : BaseVectoryGeometry<TSegment>
    where TSegment : CubicBezierSegment
{
    protected virtual void OnOpen(DrawnContext context, SKPath path)
    {
        path.MoveTo(new SKPoint(X, Y));
    }

    protected virtual void OnClose(DrawnContext context, SKPath path, TSegment segment)
    { }

    protected virtual void OnDrawSegment(DrawnContext context, SKPath path, TSegment segment)
    { }

    public override void Draw<TDrawnContext>(TDrawnContext context)
    {
        using var path = new SKPath();

        bool first = true;

        foreach (var segment in Segments)
        {
            if (first)
            {
                first = false;
                OnOpen(context, path);
            }

            OnDrawSegment(context, path, segment);
        }

        context.DrawPath(path);
    }
}


public class CubicBezierVectorGeometry : VectorGeometry<CubicBezierSegment>
{
    protected override void OnDrawSegment(DrawnContext context, SKPath path, CubicBezierSegment segment)
    {
        path.CubicTo(segment.Control1.ToSKPoint(), segment.Control2.ToSKPoint(), segment.End.ToSKPoint());
    }
}
