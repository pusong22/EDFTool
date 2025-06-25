using Core.Helper;
using Core.Kernel.Painting;
using Core.Primitive;
using SkiaSharp;

namespace EdfViewerApp.Chart;
public static class Extensions
{
    public static SKTypeface ToSKTypeface(this Paint paint)
    {
        if (paint.FontFamily is not null)
            return SKTypeface.FromFamilyName(paint.FontFamily);

        return SKTypeface.Default;
    }

    public static SKPaintStyle ToSKStyle(this Paint paint)
    {
        return paint.Style switch
        {
            PaintStyle.Fill => SKPaintStyle.Fill,
            PaintStyle.Stroke => SKPaintStyle.Stroke,
            _ => SKPaintStyle.Stroke,
        };
    }

    public static SKColor ToSKColor(this Paint paint)
    {
        var color = paint.Color;

        return new SKColor(color.Red, color.Green, color.Blue);
    }

    public static SKPoint ToSKPoint(this Point p)
    {
        return new SKPoint(p.X, p.Y);
    }

    public static SKRect ToSKRect(this Rect rect)
    {
        return new SKRect(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
    }

    public static SKPathEffect? ToSKPathEffect(this Paint paint)
    {
        if (paint.PathEffect is DashEffectSetting dashSettings)
        {
            return SKPathEffect.CreateDash(dashSettings.Intervals, dashSettings.Phase);
        }

        return null;
    }

    public static Color ToColor(this SKColor color)
    {
        return new Color(color.Red, color.Green, color.Blue, color.Alpha);
    }

    public static SKColor ToSKColor(this Color color)
    {
        return new SKColor(color.Red, color.Green, color.Blue, color.Alpha);
    }

    public static void UseDefault(this ChartConfig chartConfig)
    {
        chartConfig
            .AddValueTypeParser<short>((x, y) => new(x, y))
            .AddValueTypeParser<int>((x, y) => new(x, y))
            .AddValueTypeParser<double>((x, y) => new(x, y))
            .AddValueTypeParser<float>((x, y) => new(x, y))
            .AddValueTypeParser<long>((x, y) => new(x, y));
    }
}
