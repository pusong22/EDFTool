using EdfLib;
using EdfViewerApp.ViewModel;

namespace EdfViewerApp.Store;
public class EDFStore
{
    private EdfParser? _parser;

    public bool Open { get; private set; }
    public string? EdfFilePath { get; private set; }
    public event EventHandler? InitializeTimeRangeHandler;

    public void OpenFile(string filePath)
    {
        _parser = new EdfParser(filePath);

        Open = true;
        EdfFilePath = filePath;

        InitializeTimeRangeHandler?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerable<SignalViewModel> ReadInfo()
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        int id = 0;

        foreach (var signalInfo in _parser.Signals)
        {
            yield return new SignalViewModel()
            {
                Id = id++,
                Label = signalInfo.Label,
                SampleRate = signalInfo.SampleRate,
            };
        }
    }

    public double[] ReadPhysicalData(int index, int startRecord, int recordCount)
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        var buf = _parser.ReadSignalData(index, startRecord, recordCount);

        return buf;
    }

    public double GetTotalDurationInSeconds()
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        return _parser.NumberOfDataRecords * _parser.DurationOfDataRecordSeconds;
    }
}
