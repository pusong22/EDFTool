using System.Globalization;
using System.Text;

namespace EdfLib;

public class EdfParser : IDisposable
{
    private readonly string _filePath;
    private FileStream? _fileStream;
    private BinaryReader? _binaryReader;
    private bool _isDisposed = false;

    // --- EDF Header Fields ---
    public string Version { get; private set; } = string.Empty; // 8 ascii chars
    public string PatientIdentification { get; private set; } = string.Empty; // 80 ascii chars
    public string RecordingIdentification { get; private set; } = string.Empty; // 80 ascii chars
    public DateTime RecordingStartDateTime { get; private set; } // 8 ascii chars for date, 8 for time
    public int HeaderSizeInBytes { get; private set; } // 8 ascii chars
    public string ReservedHeader { get; private set; } = string.Empty; // 44 ascii chars (general)
    public int NumberOfDataRecords { get; private set; } // 8 ascii chars, -1 if unknown
    public double DurationOfDataRecordSeconds { get; private set; } // 8 ascii chars, in seconds
    public int NumberOfSignals { get; private set; } // 4 ascii chars

    /// <summary>
    /// A list containing metadata for each signal within the EDF file.
    /// </summary>
    public IReadOnlyList<EdfSignalInfo> Signals { get; private set; } = [];


    // Cached total samples per data record for performance
    private int _totalSamplesPerDataRecord;
    private long _dataRecordByteSize;

    public EdfParser(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
        }
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"EDF file not found: {filePath}");
        }

        _filePath = filePath;
        InitializeFileAccess();
        ParseHeader();
        CalculateDataRecordMetrics();
    }

    private void InitializeFileAccess()
    {
        try
        {
            _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _binaryReader = new BinaryReader(_fileStream, Encoding.ASCII);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Could not open EDF file '{_filePath}': {ex.Message}", ex);
        }
    }

    private void ParseHeader()
    {
        if (_fileStream is null)
            throw new NullReferenceException("_fileStream is null");

        if (_binaryReader is null)
            throw new NullReferenceException("_binaryReader is null");

        // Ensure stream is at the beginning
        _fileStream.Seek(0, SeekOrigin.Begin);

        // Read fixed-length header fields (256 bytes total for main header)
        Version = _binaryReader.ReadAsciiString(8);
        PatientIdentification = _binaryReader.ReadAsciiString(80);
        RecordingIdentification = _binaryReader.ReadAsciiString(80);

        string dateStr = _binaryReader.ReadAsciiString(8); // dd.mm.yy
        string timeStr = _binaryReader.ReadAsciiString(8); // hh.mm.ss

        if (!DateTime.TryParseExact($"{dateStr} {timeStr}", "dd.MM.yy HH.mm.ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeResult))
        {
            throw new InvalidDataException($"Could not parse recording start date/time: {dateStr} {timeStr}");
        }
        RecordingStartDateTime = dateTimeResult;

        if (!int.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Integer, CultureInfo.InvariantCulture, out int headerSize))
        {
            throw new InvalidDataException("Could not parse header size.");
        }
        HeaderSizeInBytes = headerSize;

        ReservedHeader = _binaryReader.ReadAsciiString(44);

        if (!int.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Integer, CultureInfo.InvariantCulture, out int numDataRecords))
        {
            throw new InvalidDataException("Could not parse number of data records.");
        }
        NumberOfDataRecords = numDataRecords;

        if (!double.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Float, CultureInfo.InvariantCulture, out double dataRecordDuration))
        {
            throw new InvalidDataException("Could not parse duration of data record.");
        }
        DurationOfDataRecordSeconds = dataRecordDuration; // Set the parser's property

        if (!int.TryParse(_binaryReader.ReadAsciiString(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out int numSignals))
        {
            throw new InvalidDataException("Could not parse number of signals.");
        }
        NumberOfSignals = numSignals;

        // Initialize signal info list
        var signalList = new List<EdfSignalInfo>(NumberOfSignals);
        for (int i = 0; i < NumberOfSignals; i++)
        {
            // Pass DurationOfDataRecordSeconds to the EdfSignalInfo constructor
            signalList.Add(new EdfSignalInfo(DurationOfDataRecordSeconds));
        }

        // Read signal-specific header fields (N_s * 256 bytes)
        // Each block of N_s bytes contains info for one aspect across all signals.
        // Example: all signal labels, then all transducer types, etc.

        // Labels (ns * 16 bytes)
        foreach (var signal in signalList)
            signal.Label = _binaryReader.ReadAsciiString(16);
        // Transducer type (ns * 80 bytes)
        foreach (var signal in signalList)
            signal.TransducerType = _binaryReader.ReadAsciiString(80);
        // Physical dimension (ns * 8 bytes)
        foreach (var signal in signalList)
            signal.PhysicalDimension = _binaryReader.ReadAsciiString(8);

        // Physical minimum (ns * 8 bytes)
        foreach (var signal in signalList)
        {
            if (!double.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                throw new InvalidDataException($"Could not parse physical minimum for signal {signal.Label}.");
            signal.PhysicalMinimum = val;
        }
        // Physical maximum (ns * 8 bytes)
        foreach (var signal in signalList)
        {
            if (!double.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                throw new InvalidDataException($"Could not parse physical maximum for signal {signal.Label}.");
            signal.PhysicalMaximum = val;
        }
        // Digital minimum (ns * 8 bytes)
        foreach (var signal in signalList)
        {
            if (!int.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
                throw new InvalidDataException($"Could not parse digital minimum for signal {signal.Label}.");
            signal.DigitalMinimum = val;
        }
        // Digital maximum (ns * 8 bytes)
        foreach (var signal in signalList)
        {
            if (!int.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
                throw new InvalidDataException($"Could not parse digital maximum for signal {signal.Label}.");
            signal.DigitalMaximum = val;
        }
        // Prefiltering (ns * 80 bytes)
        foreach (var signal in signalList)
            signal.Prefiltering = _binaryReader.ReadAsciiString(80);

        // Number of samples in each data record (ns * 8 bytes)
        foreach (var signal in signalList)
        {
            if (!int.TryParse(_binaryReader.ReadAsciiString(8), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
                throw new InvalidDataException($"Could not parse number of samples for signal {signal.Label}.");
            signal.NumberOfSamplesInDataRecord = val;
        }

        // Reserved for each signal (ns * 32 bytes)
        foreach (var signal in signalList)
            signal.ReservedSignal = _binaryReader.ReadAsciiString(32);

        Signals = signalList.AsReadOnly();

        // After parsing the header, verify the stream position is correct
        if (_fileStream.Position != HeaderSizeInBytes)
        {
            // This might indicate an invalid header size or malformed header data
            // For robust parsing, you might consider adjusting the stream position
            // For now, it's an indicator of potential issue.
        }
    }

    private void CalculateDataRecordMetrics()
    {
        _totalSamplesPerDataRecord = Signals.Sum(s => s.NumberOfSamplesInDataRecord);
        _dataRecordByteSize = (long)_totalSamplesPerDataRecord * 2; // 2 bytes per sample (Int16)
    }

    public List<double[]> ReadDataRecord(int recordIndex)
    {
        ValidateParserState();
        if (recordIndex < 0 || (NumberOfDataRecords != -1 && recordIndex >= NumberOfDataRecords))
        {
            throw new ArgumentOutOfRangeException(nameof(recordIndex), $"Record index {recordIndex} is out of bounds. Valid range: 0 to {NumberOfDataRecords - 1} (or -1 if unknown total records).");
        }

        // Calculate the exact byte offset to the start of the desired data record
        long offset = HeaderSizeInBytes + (recordIndex * _dataRecordByteSize);
        SeekToFileOffset(offset, $"data record {recordIndex}");

        List<double[]> recordData = [];

        try
        {
            foreach (var signal in Signals)
            {
                double[] samples = new double[signal.NumberOfSamplesInDataRecord];
                for (int i = 0; i < signal.NumberOfSamplesInDataRecord; i++)
                {
                    // EDF samples are typically 2-byte signed integers (Int16, Little-Endian)
                    short digitalValue = _binaryReader!.ReadInt16();
                    samples[i] = ScaleDigitalToPhysical(digitalValue, signal);
                }
                recordData.Add(samples);
            }
        }
        catch (EndOfStreamException ex)
        {
            throw new EndOfStreamException($"Unexpected end of file encountered while reading data record {recordIndex}. File might be incomplete or corrupt.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"An I/O error occurred while reading data record {recordIndex}: {ex.Message}", ex);
        }

        return recordData;
    }

    public double[] ReadSignalData(int signalIndex, int startRecordIndex, int numberOfRecordsToRead)
    {
        ValidateParserState();

        if (signalIndex < 0 || signalIndex >= NumberOfSignals)
        {
            throw new ArgumentOutOfRangeException(nameof(signalIndex), $"Signal index {signalIndex} is out of bounds. Valid range: 0 to {NumberOfSignals - 1}.");
        }
        if (startRecordIndex < 0 || (NumberOfDataRecords != -1 && startRecordIndex >= NumberOfDataRecords))
        {
            throw new ArgumentOutOfRangeException(nameof(startRecordIndex), $"Start record index {startRecordIndex} is out of bounds. Valid range: 0 to {NumberOfDataRecords - 1} (or -1 if unknown total records).");
        }

        EdfSignalInfo targetSignal = Signals[signalIndex];
        int samplesPerSignalPerRecord = targetSignal.NumberOfSamplesInDataRecord;

        // Determine actual number of records to read
        long actualRecordsToRead = numberOfRecordsToRead;
        if (numberOfRecordsToRead == -1)
        {
            // If -1, read all remaining records
            if (NumberOfDataRecords != -1)
            {
                actualRecordsToRead = NumberOfDataRecords - startRecordIndex;
            }
            else
            {
                // If total records are unknown, we must estimate or read until EOF.
                // For safety and to pre-allocate, let's assume we read until EOF based on file size.
                // This is an estimation and might be slightly off if file is truncated/padded.
                long remainingBytes = _fileStream!.Length - (HeaderSizeInBytes + startRecordIndex * _dataRecordByteSize);
                actualRecordsToRead = remainingBytes / _dataRecordByteSize;

                if (actualRecordsToRead < 0) actualRecordsToRead = 0; // Guard against negative
            }
        }
        else if (NumberOfDataRecords != -1)
        {
            // Ensure we don't try to read beyond the end of the file
            actualRecordsToRead = Math.Min(actualRecordsToRead, NumberOfDataRecords - startRecordIndex);
        }

        if (actualRecordsToRead <= 0) return []; // No records to read

        int totalSamplesToRead = (int)(actualRecordsToRead * samplesPerSignalPerRecord);
        if (totalSamplesToRead == 0) return [];

        double[] resultSamples = new double[totalSamplesToRead];
        int currentSampleIndex = 0;

        // Loop through each data record we need to process
        for (long recordIdx = startRecordIndex; recordIdx < startRecordIndex + actualRecordsToRead; recordIdx++)
        {
            // Calculate the start offset for the current data record
            long recordStartOffset = HeaderSizeInBytes + (recordIdx * _dataRecordByteSize);
            SeekToFileOffset(recordStartOffset, $"data record {recordIdx} for signal {signalIndex}");

            // Iterate through all signals within this data record to find the target signal's samples
            long currentSignalOffsetInRecord = 0; // Bytes offset from recordStartOffset
            for (int sIdx = 0; sIdx < NumberOfSignals; sIdx++)
            {
                EdfSignalInfo currentSignalInRecord = Signals[sIdx];
                int samplesInCurrentBlock = currentSignalInRecord.NumberOfSamplesInDataRecord;
                int bytesInCurrentBlock = samplesInCurrentBlock * 2; // Each sample is 2 bytes (short)

                if (sIdx == signalIndex)
                {
                    // This is our target signal. Read its samples.
                    for (int i = 0; i < samplesInCurrentBlock; i++)
                    {
                        short digitalValue = _binaryReader!.ReadInt16();
                        resultSamples[currentSampleIndex++] = ScaleDigitalToPhysical(digitalValue, targetSignal);
                    }
                }
                else
                {
                    // This is not our target signal. Skip its samples efficiently.
                    // We can't just seek _binaryReader.BaseStream by bytesInCurrentBlock
                    // because BinaryReader maintains its own buffer.
                    // The most reliable way is to read and discard, or optimize by
                    // directly manipulating the underlying stream *only if* it's guaranteed
                    // BinaryReader won't get confused. For simplicity and robustness, read and discard.
                    _binaryReader!.ReadBytes(bytesInCurrentBlock);
                }
                currentSignalOffsetInRecord += bytesInCurrentBlock;
            }
        }

        return resultSamples;
    }

    private void ValidateParserState()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(EdfParser), "Cannot perform operations on a disposed parser.");
        if (_fileStream == null || !_fileStream.CanRead)
            throw new InvalidOperationException("File stream is not open or cannot be read.");
        if (_binaryReader == null)
            throw new InvalidOperationException("BinaryReader is not access.");
    }

    private void SeekToFileOffset(long offset, string context)
    {
        try
        {
            // Set the stream position directly. BinaryReader will adapt.
            _fileStream!.Seek(offset, SeekOrigin.Begin);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Error seeking to {context}: {ex.Message}", ex);
        }
    }

    private double ScaleDigitalToPhysical(short digitalValue, EdfSignalInfo signalInfo)
    {
        // Avoid division by zero if digital range is 0 (though unlikely in valid EDF)
        if (signalInfo.DigitalMaximum == signalInfo.DigitalMinimum)
        {
            // If digital range is zero, assume no scaling or a constant value.
            // This case might indicate a malformed signal definition.
            // For robustness, return physical minimum or throw an error.
            // Here, we return physical minimum as a safe default.
            return signalInfo.PhysicalMinimum;
        }

        double gain = signalInfo.PhysicalRange / signalInfo.DigitalRange;
        double offset = (signalInfo.PhysicalMaximum / gain - signalInfo.DigitalMaximum);
        //double offset = signalInfo.PhysicalMinimum - gain * signalInfo.DigitalMinimum;
        return gain * (digitalValue + offset);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Prevent the finalizer from running
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            // Dispose managed state (managed objects)
            _binaryReader?.Dispose();
            _fileStream?.Dispose();
        }

        // Free unmanaged resources (unmanaged objects) and override finalizer
        // Set large fields to null
        _binaryReader = null;
        _fileStream = null;
        Signals = []; // Clear the reference

        _isDisposed = true;
    }

    ~EdfParser()
    {
        Dispose(false);
    }
}
