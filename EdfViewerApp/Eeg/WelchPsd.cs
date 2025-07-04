using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace EdfViewerApp.Eeg;

public class WelchPSD
{
    public static (double[] frequencies, double[] psd) Compute(
        double[] x,
        double fs = 1.0,
        string windowType = "hann",
        int nperseg = 256,
        int noverlap = -1,
        int nfft = -1,
        string scaling = "density")
    {
        var result = ComputeInternal(x, fs, windowType, nperseg, noverlap, nfft, scaling);
        return (result.frequencies, result.psd);
    }

    public static (double[] frequencies, double[] times, double[,] spectrogram) ComputeSpectrogram(
        double[] x,
        double fs = 1.0,
        string windowType = "hann",
        int nperseg = 256,
        int noverlap = -1,
        int nfft = -1,
        string scaling = "density")
    {
        var result = ComputeInternal(x, fs, windowType, nperseg, noverlap, nfft, scaling);
        return (result.frequencies, result.times, result.spectrogram);
    }

    private static (
        double[] frequencies,
        double[] psd,
        double[] times,
        double[,] spectrogram
        ) ComputeInternal(
        double[] x,
        double fs,
        string windowType,
        int nperseg,
        int noverlap,
        int nfft,
        string scaling)
    {
        // 参数验证与默认值设置
        if (noverlap < 0)
            noverlap = nperseg / 2;  // 默认50%重叠
        if (nfft <= 0)
            nfft = nperseg;         // 默认FFT长度等于段长
        if (nfft < nperseg)
            throw new ArgumentException("nfft must be greater than or equal to nperseg");

        // 生成窗函数
        double[] window = GenerateWindow(windowType, nperseg);
        double windowPower = window.Select(w => w * w).Sum();

        int step = nperseg - noverlap;
        int numSegments = (x.Length - nperseg) / step + 1;

        double[] freqs = new double[nfft / 2 + 1];
        double[] psd = new double[freqs.Length];
        double[] times = new double[numSegments];
        double[,] spectrogram = new double[numSegments, freqs.Length];

        // 计算归一化因子
        double scale;
        if (scaling.ToLower() == "density")
            scale = 1.0 / (fs * windowPower);
        else // spectrum
            scale = 1.0 / (window.Sum() * window.Sum());

        bool isEvenNfft = nfft % 2 == 0;

        for (int i = 0; i < numSegments; i++)
        {
            times[i] = (i * step) / fs;// + nperseg / 2.0) / fs;

            double[] segment = new double[nperseg];
            Array.Copy(x, i * step, segment, 0, nperseg);

            ApplyWindow(segment, window);

            Complex[] padded = new Complex[nfft];
            for (int j = 0; j < nperseg; j++)
                padded[j] = new Complex(segment[j], 0);

            Fourier.Forward(padded, FourierOptions.NoScaling);

            for (int k = 0; k < freqs.Length; k++)
            {
                double magnitude = padded[k].MagnitudeSquared();

                bool isDC = (k == 0);
                bool isNyquist = (isEvenNfft && k == nfft / 2);
                if (!isDC && !isNyquist)
                    magnitude *= 2;

                magnitude *= scale;

                spectrogram[i, k] = 10 * Math.Log10(magnitude + 1e-12);

                psd[k] += magnitude;
            }
        }

        psd = [.. psd.Select(p => p / numSegments)];

        double fftResolution = fs / nfft;
        for (int i = 0; i < freqs.Length; i++)
            freqs[i] = i * fftResolution;

        return (freqs, psd, times, spectrogram);
    }

    private static double[] GenerateWindow(string type, int length)
    {
        return type.ToLower() switch
        {
            "hann" => Window.Hann(length),
            "hamming" => Window.Hamming(length),
            "blackman" => Window.Blackman(length),
            "flattop" => Window.FlatTop(length),
            "rectangular" => Window.Dirichlet(length),
            _ => throw new ArgumentException($"Unsupported window type: {type}")
        };
    }

    private static void ApplyWindow(double[] segment, double[] window)
    {
        for (int i = 0; i < segment.Length; i++)
            segment[i] *= window[i];
    }
}
