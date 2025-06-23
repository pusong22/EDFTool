using Core.Kernel.Drawing;
using Core.Kernel.Drawing.Geometry;
using Core.Kernel.Painting;
using SkiaSharp;

namespace EDFToolApp.Chart.Drawing;
public class SkiaSharpDrawnContext(SKSurface surface, SKImageInfo info)
    : DrawnContext
{
    public SKCanvas Canvas { get; } = surface.Canvas;
    public SKImageInfo Info { get; } = info;
    public SKPaint? ActivateSkPaint { get; protected internal set; }
    public SKFont? ActivateSkFont { get; protected internal set; }

    public override void BeginDraw()
    {
        Canvas.Clear();
        using var p = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(255, 255, 255)
        };

        Canvas.DrawRect(Info.Rect, p);
    }

    public override void Draw(DrawnGeometry drawable)
    {
        Canvas.Save();

        var p = new SKPoint(drawable.X, drawable.Y);

        if (drawable.HasRotation)
        {
            // 先更新旋转起点，然在更新绘制起点
            Canvas.Translate(p.X, p.Y);
            Canvas.RotateDegrees(drawable.RotateTransform);
            Canvas.Translate(-p.X, -p.Y);
        }

        ActivateSkPaint!.Color =
            ActivateSkPaint.Color.WithAlpha((byte)(255 * drawable.Opacity));


        drawable.Draw(this);

        Canvas.Restore();
    }

    public override void DisposePaint()
    {
        ActivateSkPaint?.Dispose();
        ActivateSkFont?.Dispose();

        ActivateSkPaint = null;
        ActivateSkFont = null;
    }

    public override void InitializePaint(Paint paint)
    {
        ActivateSkPaint ??= new SKPaint();

        ActivateSkPaint.Color = paint.ToSKColor();
        ActivateSkPaint.IsAntialias = paint.IsAntialias;

        ActivateSkPaint.Style = paint.ToSKStyle();

        ActivateSkPaint.PathEffect = paint.ToSKPathEffect();

        ActivateSkFont ??= new SKFont();

        ActivateSkFont.Typeface = paint.ToSKTypeface();
        ActivateSkFont.Size = paint.FontSize;
    }

    public override TRect MeasureText<TRect>(string text)
    {
        ActivateSkFont!.MeasureText(text, out var bound, ActivateSkPaint!);

        return (TRect)(object)bound;
    }

    public override void DrawRect<TRect>(TRect rect)
    {
        if (rect is not SKRect skRect) return;

        Canvas.DrawRect(skRect, ActivateSkPaint!);
    }

    public override void DrawText<TPoint>(string text, TPoint p)
    {
        if (p is not SKPoint skPoint) return;

        Canvas.DrawText(text, skPoint, ActivateSkFont!, ActivateSkPaint!);
    }

    public override void DrawLine<TPoint>(TPoint p1, TPoint p2)
    {
        if (p1 is not SKPoint skPoint1) return;
        if (p2 is not SKPoint skPoint2) return;

        Canvas.DrawLine(skPoint1, skPoint2, ActivateSkPaint!);
    }

    public override void DrawCircle<TPoint>(TPoint p, float rd)
    {
        if (p is not SKPoint skPoint) return;

        Canvas.DrawCircle(skPoint, rd, ActivateSkPaint!);
    }

    public override void DrawPath<TPath>(TPath path)
    {
        if (path is not SKPath skPath) return;

        Canvas.DrawPath(skPath, ActivateSkPaint!);
    }
}
