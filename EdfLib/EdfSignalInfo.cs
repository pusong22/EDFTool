namespace EdfLib;

/// <summary>
/// Represents the metadata for a single signal (channel) within an EDF file.
/// </summary>
/// <remarks>
/// Initializes a new instance of the EdfSignalInfo class.
/// </remarks>
/// <param name="durationOfDataRecordSeconds">The duration of a single data record in seconds, obtained from the EDF file header.</param>
public class EdfSignalInfo(double durationOfDataRecordSeconds)
{
    public string Label { get; set; } = string.Empty; // 16 ascii chars
    public string TransducerType { get; set; } = string.Empty; // 80 ascii chars
    public string PhysicalDimension { get; set; } = string.Empty; // 8 ascii chars (e.g., "uV", "mmHg")
    public double PhysicalMinimum { get; set; } // 8 ascii chars
    public double PhysicalMaximum { get; set; } // 8 ascii chars
    public int DigitalMinimum { get; set; } // 8 ascii chars
    public int DigitalMaximum { get; set; } // 8 ascii chars
    public string Prefiltering { get; set; } = string.Empty; // 80 ascii chars
    public int NumberOfSamplesInDataRecord { get; set; } // 8 ascii chars
    public string ReservedSignal { get; set; } = string.Empty; // 32 ascii chars (per signal)

    /// <summary>
    /// Gets the sample rate for this signal in Hz (samples per second).
    /// Calculated as NumberOfSamplesInDataRecord / DurationOfDataRecordSeconds.
    /// </summary>
    public double SampleRate => durationOfDataRecordSeconds > 0d ? NumberOfSamplesInDataRecord / durationOfDataRecordSeconds : 0d;
    public double DigitalRange => DigitalMaximum - DigitalMinimum;
    public double PhysicalRange => PhysicalMaximum - PhysicalMinimum;
}
