using EDFLibSharp;
using EDFToolApp.ViewModel;
using Service;
using System.Text;

namespace EDFToolApp.Store;
public class EDFStore(EDFService edfService)
{
    public void OpenFile(string filePath)
    {
        edfService.Initialize(filePath);
    }

    public IEnumerable<SignalViewModel> ReadInfo()
    {
        HeaderInfo headerInfo = edfService.ReadHeaderInfo();
        uint count = headerInfo._signalCount;
        SignalInfo[] signalInfos = edfService.ReadSignalInfo(count);

        foreach (SignalInfo signalInfo in signalInfos)
        {
            yield return new SignalViewModel() { Label = ClipLabel(signalInfo._label) };
        }
    }

    private string ClipLabel(char[] label)
    {
        StringBuilder sb = new();
        foreach (char c in label)
        {
            if (c == '\0')
                break;

            sb.Append(c);
        }

        return sb.ToString();
    }
}
