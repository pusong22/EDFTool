using MathNet.Filtering.FIR;
using MathNet.Numerics.IntegralTransforms;

namespace EdfViewerApp.Eeg;
public class EegProcessor
{
    public static double[] ApplyBandpassFilter(double[] signal,
        double sampleRate, double lowCutOff, double highCutoff, int halfOrder = 0)
    {
        double[] coefficients =  FirCoefficients.BandPass(sampleRate, lowCutOff, highCutoff, halfOrder);
        OnlineFirFilter filter = new (coefficients);

        var output = new double[signal.Length];
        for (int i = 0; i < signal.Length; i++)
        {
            output[i] = filter.ProcessSample(signal[i]);
        }

        return output;
    }

    public static List<double[]> SegmentSignal(double[] signal, 
        double sampleRate, double epochDurationSeconds)
    {
        int samplesPerEpoch = (int)(sampleRate * epochDurationSeconds);
        List<double[]> epochs = [];

        for (int start = 0; start + samplesPerEpoch <= signal.Length; start += samplesPerEpoch)
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

        Fourier.Forward(complexSamples, FourierOptions.Matlab);

        // 计算功率谱（幅度平方）
        double[] powerSpectrum = new double[n / 2];
        for (int i = 0; i < n / 2; i++)
        {
            powerSpectrum[i] = complexSamples[i].Magnitude * complexSamples[i].Magnitude;
        }
        return powerSpectrum;
    }

    public static double[] GetFrequencyBins(int fftLength, double sampleRate)
    {
        double freqResolution = sampleRate / fftLength;
        double[] freqBins = new double[fftLength / 2];
        for (int i = 0; i < fftLength / 2; i++)
        {
            freqBins[i] = i * freqResolution;
        }
        return freqBins;
    }

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
}

public class BandPower
{
    public double Delta;
    public double Theta;
    public double Alpha;
    public double Beta;
    public double Gamma;
}

