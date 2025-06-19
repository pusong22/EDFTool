using Core.Kernel.Drawing.Geometry;
using Core.Primitive;
using SkiaSharp;

namespace EDFToolApp.Chart.Drawing.Geometry;
public class LabelGeometry : BaseLabelGeometry
{
    private float _maxTextHeight = 0f;
    private int _lines = 0;

    public override void Draw<TDrawnContext>(TDrawnContext context)
    {
        // 因为是每次画一行，初始化的x,y是中心点，经测试发现skia字体逻辑是
        // Align.Left, Align.Bottom，
        float h = _lines > 1
                    ? VerticalAlign switch
                    {
                        Align.Start => 0f,
                        Align.Middle => -(_lines - 1) * _maxTextHeight * LineHeight * 0.5f,
                        Align.End => -(_lines - 1) * _maxTextHeight * LineHeight,
                        _ => 0f
                    }
                    : 0f;

        foreach (var line in GetLines())
        {
            var bound = context.MeasureText<SKRect>(line);

            var xo = GetAlignmentOffset(bound);

            context.DrawText(line, new SKPoint(X + xo.X, Y + xo.Y + h));

            h += _maxTextHeight * LineHeight;
        }
    }

    public override Size Measure()
    {
        if (Paint is null)
            throw new ArgumentNullException(nameof(Paint));

        using var font = new SKFont
        {
            Size = TextSize,
            Typeface = Paint.ToSKTypeface()
        };

        var w = 0f;
        _maxTextHeight = 0f;
        _lines = 0;

        foreach (var line in GetLines())
        {
            font.MeasureText(line, out var bound);

            if (bound.Width > w) w = bound.Width;
            if (bound.Height > _maxTextHeight) _maxTextHeight = bound.Height;
            _lines++;
        }

        var h = _maxTextHeight * _lines * LineHeight;

        var padding = Padding ?? new Padding(0f);

        var size = new Size(
            w + padding.Left + padding.Right,
            h + padding.Top + padding.Bottom);

        return size.GetRotatedSize(RotateTransform);
    }


    private IEnumerable<string> GetLines()
    {
        if (Text is null)
            throw new ArgumentNullException(nameof(Text));

        IEnumerable<string> lines = Text.Split([Environment.NewLine], StringSplitOptions.None);

        return lines;
    }


    private SKPoint GetAlignmentOffset(SKRect bounds)
    {
        var w = bounds.Width;
        var h = bounds.Height;

        float l = -bounds.Left, t = -bounds.Top;

        switch (VerticalAlign)
        {
            case Align.Start: t += 0; break;
            case Align.Middle: t -= h * 0.5f; break;
            case Align.End: t -= h + 0; break;
            default:
                break;
        }
        switch (HorizontalAlign)
        {
            case Align.Start: l += 0; break;
            case Align.Middle: l -= w * 0.5f; break;
            case Align.End: l -= w + 0; break;
            default:
                break;
        }

        return new(l, t);
    }
}
