using EDFLibSharp;

namespace Service
{
    public class EDFService
    {
        private EDFReader? _reader;

        public void Initialize(string edfFilePath)
        {
            _reader?.Dispose();
            _reader = new EDFReader(edfFilePath);
        }


        public HeaderInfo ReadHeaderInfo()
        {
            if (_reader is null) throw new Exception("EdfService don`t be initialize!");

            return _reader.ReadHeader();
        }

        public SignalInfo[] ReadSignalInfo(uint count)
        {
            if (_reader is null) throw new Exception("EdfService don`t be initialize!");

            return _reader.ReadSignalInfo(count);
        }
    }
}
