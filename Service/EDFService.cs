using EDFLibSharp;

namespace EdfBrowser.Services
{
    public class EDFService
    {
        private EDFReader? _reader;

        public void Initialize(string edfFilePath)
        {
            _reader?.Dispose();
            _reader = new EDFReader(edfFilePath);
        }
    }
}
