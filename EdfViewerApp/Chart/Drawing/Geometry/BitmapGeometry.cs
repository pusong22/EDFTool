using Core.Kernel.Drawing.Geometry;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace EdfViewerApp.Chart.Drawing.Geometry;
public class BitmapGeometry : BaseBitmapGeometry
{
    public override void Draw<TDrawnContext>(TDrawnContext context)
    {
        if (PixelData is null) return;

        var info = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var bitmap = new SKBitmap(info);
        Marshal.Copy(PixelData, 0, bitmap.GetPixels(), PixelData.Length);

        context.DrawBitmap(bitmap, DestRect.ToSKRect());
    }
}
