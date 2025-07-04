using Core.Primitive;

namespace EdfViewerApp;

public static class HelperUtils
{
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        else if (value.CompareTo(max) > 0) return max;
        else return value;
    }

    public static IEnumerable<Coordinate> LoadMonaLisaPoints(
        string imagePath, int targetWidth, int targetHeight)
    {
        // 加载图片
        var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
        bitmapImage.DecodePixelWidth = targetWidth; // 缩放宽度
        bitmapImage.DecodePixelHeight = targetHeight; // 缩放高度
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        // 转成 FormatConvertedBitmap 确保格式为 Bgra32 (每像素4字节)
        var formattedBitmap = new System.Windows.Media.Imaging.FormatConvertedBitmap(bitmapImage, System.Windows.Media.PixelFormats.Bgra32, null, 0);
        formattedBitmap.Freeze();

        int width = formattedBitmap.PixelWidth;
        int height = formattedBitmap.PixelHeight;
        int stride = width * 4; // 每行字节数

        var pixels = new byte[height * stride];
        formattedBitmap.CopyPixels(pixels, stride, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * stride + x * 4;
                byte b = pixels[idx + 0];
                byte g = pixels[idx + 1];
                byte r = pixels[idx + 2];
                // byte a = pixels[idx + 3]; // 如果需要 alpha

                // 计算亮度（灰度），和你原来权重一样
                double z = (r * 0.3 + g * 0.59 + b * 0.11) / 255.0;

                yield return new Coordinate(x, y, z);
            }
        }
    }
}
