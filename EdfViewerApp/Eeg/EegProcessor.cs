using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace EdfViewerApp.Eeg;
public class EegProcessor
{
    public static IEnumerable<double[]> SegmentSignal(double[] signal, int samplesPerEpoch, int overlapSamples)
    {
        if (signal == null)
            throw new ArgumentNullException(nameof(signal));
        if (samplesPerEpoch <= 0)
            throw new ArgumentOutOfRangeException(nameof(samplesPerEpoch), "Samples per epoch must be positive.");
        if (overlapSamples < 0 || overlapSamples >= samplesPerEpoch)
            throw new ArgumentOutOfRangeException(nameof(overlapSamples), "Overlap must be non-negative and less than samples per epoch.");

        int step = samplesPerEpoch - overlapSamples;

        for (int start = 0; start < signal.Length; start += step)
        {
            double[] epoch = new double[samplesPerEpoch]; // 初始化默认全是 0
            int remaining = signal.Length - start;
            int copyLength = Math.Min(samplesPerEpoch, remaining);

            Array.Copy(signal, start, epoch, 0, copyLength);
            yield return epoch;
        }
    }

    public static double[] ComputePowerSpectrum(double[] signal)
    {
        if (signal == null)
            throw new ArgumentNullException(nameof(signal));
        if (signal.Length == 0)
            throw new ArgumentException("Signal cannot be empty.", nameof(signal));

        int n = signal.Length;
        // 复数数组，实部是信号，虚部为0
        var complexSamples = new System.Numerics.Complex[n];
        for (int i = 0; i < n; i++)
            complexSamples[i] = new System.Numerics.Complex(signal[i], 0);

        Fourier.Forward(complexSamples, FourierOptions.Matlab);

        // 计算功率谱（幅度平方）
        int numUniqueFrequencies = n / 2 + 1;
        double[] powerSpectrum = new double[numUniqueFrequencies];
        // DC 分量 (0 Hz) - 不乘以 2
        powerSpectrum[0] = complexSamples[0].MagnitudeSquared();
        // 正频率分量 (不包括 Nyquist 频率) - 乘以 2
        for (int i = 1; i < n / 2; i++)
            powerSpectrum[i] = 2 * complexSamples[i].MagnitudeSquared();
        // 奈奎斯特频率分量 (如果 FFT 长度是偶数) - 不乘以 2
        if (n % 2 == 0)
            powerSpectrum[n / 2] = complexSamples[n / 2].MagnitudeSquared();
      
        return powerSpectrum;
    }

    public static double[] GetFrequencyBins(int fftLength, double sampleRate)
    {
        double freqResolution = sampleRate / fftLength;
        int numUniqueFrequencies = fftLength / 2 + 1; // 包含直流和奈奎斯特频率
        double[] freqBins = new double[numUniqueFrequencies];
        for (int i = 0; i < numUniqueFrequencies; i++)
            freqBins[i] = i * freqResolution;

        return freqBins;
    }

    public static double[] GenerateHanningWindow(int size)
    {
        return Window.Hann(size);
    }

    // 时频
    public static SpectrogramResult ComputeSpectrogram(
        double[] signal,
        double sampleRate,
        int samplesPerEpoch, // epoch length in samples
        int overlapSamples)
    {
        if (signal == null || signal.Length == 0)
            throw new ArgumentException("Signal cannot be null or empty.", nameof(signal));
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive.");
        if (samplesPerEpoch <= 0 || (samplesPerEpoch & (samplesPerEpoch - 1)) != 0)
            throw new ArgumentException("samplesPerEpoch must be a power of 2 and positive.", nameof(samplesPerEpoch));
        if (overlapSamples < 0 || overlapSamples >= samplesPerEpoch)
            throw new ArgumentException("overlapSamples must be non-negative and less than samplesPerEpoch.", nameof(overlapSamples));

        var segments = SegmentSignal(signal, samplesPerEpoch, overlapSamples).ToList();
        int timeSteps = segments.Count;
        int fftLength = segments[0].Length;
        int freqSteps = fftLength / 2 + 1;

        var window = GenerateHanningWindow(fftLength);
        double[,] spectrogram = new double[freqSteps, timeSteps];
      
        double[] frequenciesHz = GetFrequencyBins(fftLength, sampleRate);
        double[] timesSeconds = new double[timeSteps];

        // Calculate the actual time step duration based on samples and overlap
        double stepDurationSeconds = (double)(samplesPerEpoch - overlapSamples) / sampleRate;
        for (int t = 0; t < timeSteps; t++)
            timesSeconds[t] = t * stepDurationSeconds; // 每个时间步的起始时间

        var windowed = new double[fftLength];
        for (int t = 0; t < timeSteps; t++)
        {
            var segment = segments[t];
           
            for (int i = 0; i < fftLength; i++)
                windowed[i] = segment[i] * window[i];

            var powerSpectrum = ComputePowerSpectrum(windowed);
            const double epsilon = 1e-10;
            for (int f = 0; f < freqSteps; f++)
                spectrogram[f, t] = 10 * Math.Log10(powerSpectrum[f] + epsilon);

        }

        return new SpectrogramResult
        {
            SpectrogramData = spectrogram,
            FrequenciesHz = frequenciesHz,
            TimesSeconds = timesSeconds,
        };
    }

    #region 频段
    public static BandPower CalculateBandPower(double[] powerSpectrum, double[] freqBins)
    {
        BandPower bp = new();

        for (int i = 0; i < powerSpectrum.Length; i++)
        {
            double f = freqBins[i];
            double power = powerSpectrum[i];

            if (f >= 0.5 && f < 4) bp.Delta += power;
            else if (f >= 4 && f < 8) bp.Theta += power;
            else if (f >= 8 && f < 12) bp.Alpha += power;
            else if (f >= 12 && f < 30) bp.Beta += power;
            else if (f >= 30 && f <= 45) bp.Gamma += power;
        }

        return bp;
    }

    public static BandPower CalculateRelativePower(BandPower bp)
    {
        double totalPower = bp.Delta + bp.Theta + bp.Alpha + bp.Beta + bp.Gamma;
        if (totalPower == 0) return bp;

        return new BandPower
        {
            Delta = bp.Delta / totalPower,
            Theta = bp.Theta / totalPower,
            Alpha = bp.Alpha / totalPower,
            Beta = bp.Beta / totalPower,
            Gamma = bp.Gamma / totalPower,
        };
    }
    #endregion
}

public class SpectrogramResult
{
    public double[,]? SpectrogramData { get; set; } // 存储功率（已对数变换和归一化）
    public double[]? FrequenciesHz { get; set; }     // 存储每个频率bin对应的Hz值
    public double[]? TimesSeconds { get; set; }      // 存储每个时间步对应的秒值
}

public class BandPower
{
    public double Delta;
    public double Theta;
    public double Alpha;
    public double Beta;
    public double Gamma;
}

