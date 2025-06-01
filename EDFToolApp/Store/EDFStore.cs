using Service;

namespace EDFToolApp.Store;
public class EDFStore(EDFService edfService)
{
    public void OpenFile(string filePath)
    {
        edfService.Initialize(filePath);
    }
}
