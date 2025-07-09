using EdfLib;
using EdfViewerApp.ViewModel;

namespace EdfViewerApp.Store;
public class EDFStore
{
    private EdfParser? _parser;
    public List<SignalViewModel> SignalVMs { get; private set; } = [];

    public bool Open { get; private set; }
    public string? EdfFilePath { get; private set; }

    public void OpenFile(string filePath)
    {
        _parser = new EdfParser(filePath);

        Open = true;
        EdfFilePath = filePath;

        int id = 0;

        SignalVMs.Clear();
        foreach (var signalInfo in _parser.Signals)
        {
            SignalViewModel vm = new()
            {
                Id = id++,
                Label = signalInfo.Label,
                SampleRate = signalInfo.SampleRate,
            };

            SignalVMs.Add(vm);
        }
    }

    public async Task<double[]> ReadPhysicalData(int index,
        int startRecord = -1,
        int recordCount = -1)
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        if (startRecord < 0) startRecord = 0;
        if (recordCount < 0) recordCount = _parser.NumberOfDataRecords;

        return await ReadInternal(index, startRecord, recordCount);
    }

    public double GetTotalDurationInSeconds()
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        return _parser.NumberOfDataRecords * _parser.DurationOfDataRecordSeconds;
    }

    private async Task<double[]> ReadInternal(int index, int startRecord, int recordCount)
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        return await Task.Run(() => _parser.ReadSignalData(index, startRecord, recordCount));
    }
}
