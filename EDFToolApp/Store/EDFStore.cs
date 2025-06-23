using EDFLibSharp;
using EDFToolApp.ViewModel;

namespace EDFToolApp.Store;
public class EDFStore
{
    private EdfParser? _parser;

    public bool Open { get; private set; }
    public string? EdfFilePath { get; private set; }

    public void OpenFile(string filePath)
    {
        _parser = new EdfParser(filePath);

        Open = true;
        EdfFilePath = filePath;
    }

    public IEnumerable<SignalViewModel> ReadInfo()
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        int id = 0;

        foreach (var signalInfo in _parser.Signals)
        {
            yield return new SignalViewModel() { Id = id++, Label = signalInfo.Label };
        }
    }

    public double[] ReadPhysicalData(int index, int startRecord, int recordCount)
    {
        if (_parser is null)
            throw new InvalidOperationException("_parser is not open.");

        var buf = _parser.ReadSignalData(index, startRecord, recordCount);

        return buf;
    }
}
