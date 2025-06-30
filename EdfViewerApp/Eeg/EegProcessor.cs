using MathNet.Filtering.FIR;
using MathNet.Filtering.Windowing;
using MathNet.Numerics.IntegralTransforms;

namespace EdfViewerApp.Eeg;
public class EegProcessor
{
    public static List<double[]> SegmentSignal(double[] signal, int samples, int overlapSamples)
    {
        int samplesPerEpoch = samples;
        int stepSamples = samplesPerEpoch - overlapSamples;
        if (stepSamples <= 0) // 确保步长至少为1，避免无限循环或过小步长
            stepSamples = 1;

        List<double[]> epochs = [];

        for (int start = 0; start + samplesPerEpoch <= signal.Length; start += stepSamples)
        {
            double[] epoch = new double[samplesPerEpoch];
            Array.Copy(signal, start, epoch, 0, samplesPerEpoch);
            epochs.Add(epoch);
        }
        return epochs;
    }

    public static double[] ComputePowerSpectrum(double[] signal)
    {
        int n = signal.Length;
        // MathNet要求复数数组，实部是信号，虚部为0
        var complexSamples = new System.Numerics.Complex[n];
        for (int i = 0; i < n; i++)
            complexSamples[i] = new System.Numerics.Complex(signal[i], 0);

        Fourier.Forward(complexSamples);

        // 计算功率谱（幅度平方）
        double[] powerSpectrum = new double[n / 2];
        for (int i = 0; i < n / 2; i++)
            powerSpectrum[i] = complexSamples[i].Magnitude * complexSamples[i].Magnitude;

        return powerSpectrum;
    }

    public static double[] GetFrequencyBins(int fftLength, double sampleRate)
    {
        double freqResolution = sampleRate / fftLength;
        double[] freqBins = new double[fftLength / 2];
        for (int i = 0; i < fftLength / 2; i++)
            freqBins[i] = i * freqResolution;

        return freqBins;
    }

    public static double[] GenerateHanningWindow(int size)
    {
        var hann = new HannWindow()
        {
            Width = size,
        };
        return hann.CopyToArray();
    }

    // 时频
    public static SpectrogramResult ComputeSpectrogram(
        double[] signal,
        double sampleRate,
        int samplesPerEpoch, // epoch length in samples
        int overlapSamples,
        double lowCutOff,
        double highCutOff,
        int halfOrder = 128)
    {
        var segments = SegmentSignal(signal, samplesPerEpoch, overlapSamples);
        int timeSteps = segments.Count;
        int fftLength = segments[0].Length;
        int freqSteps = fftLength / 2;

        var window = GenerateHanningWindow(fftLength);
        double[,] spectrogram = new double[freqSteps, timeSteps];
        double[] filterCoefficients = FirCoefficients.BandPass(sampleRate, lowCutOff, highCutOff, halfOrder);
        OnlineFirFilter segmentFilter = new(filterCoefficients);

        double[] frequenciesHz = GetFrequencyBins(fftLength, sampleRate);
        double[] timesSeconds = new double[timeSteps];

        // Calculate the actual time step duration based on samples and overlap
        double stepDurationSeconds = (double)(samplesPerEpoch - overlapSamples) / sampleRate;
        for (int t = 0; t < timeSteps; t++)
        {
            timesSeconds[t] = t * stepDurationSeconds; // 每个时间步的起始时间
        }

        for (int t = 0; t < timeSteps; t++)
        {
            var segment = segments[t];
            // filter
            segmentFilter.Reset();
            var filteredSegment = new double[segment.Length];
            for (int i = 0; i < segment.Length; i++)
            {
                filteredSegment[i] = segmentFilter.ProcessSample(segment[i]);
            }

            var windowed = new double[fftLength];
            for (int i = 0; i < fftLength; i++)
            {
                windowed[i] = filteredSegment[i] * window[i];
            }

            var powerSpectrum = ComputePowerSpectrum(windowed);
            for (int f = 0; f < freqSteps; f++)
            {
                spectrogram[f, t] = Math.Log10(powerSpectrum[f] + 1e-10); // 对数功率，避免负无穷
            }
        }

        double minPowerValue = spectrogram.Cast<double>().Min();
        double maxPowerValue = spectrogram.Cast<double>().Max();

        double[,] normalizedSpectrogram = new double[freqSteps, timeSteps];
        for (int f = 0; f < freqSteps; f++)
        {
            for (int t = 0; t < timeSteps; t++)
            {
                if (maxPowerValue == minPowerValue)
                    normalizedSpectrogram[f, t] = 0.5; // 或其他默认值
                else
                    normalizedSpectrogram[f, t] = (spectrogram[f, t] - minPowerValue) / (maxPowerValue - minPowerValue);
            }
        }

        return new SpectrogramResult
        {
            SpectrogramData = normalizedSpectrogram,
            FrequenciesHz = frequenciesHz,
            TimesSeconds = timesSeconds,
            MinPower = minPowerValue,
            MaxPower = maxPowerValue
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
    public double MinPower { get; set; }           // 归一化前的最小功率值
    public double MaxPower { get; set; }           // 归一化前的最大功率值
}

public class BandPower
{
    public double Delta;
    public double Theta;
    public double Alpha;
    public double Beta;
    public double Gamma;
}

