using System.Text;

namespace EdfLib;

public static class BinaryReaderExtensions
{
    public static string ReadAsciiString(this BinaryReader reader, int length)
    {
        if (reader == null) throw new ArgumentNullException(nameof(reader));
        if (length <= 0) return string.Empty;

        byte[] bytes = reader.ReadBytes(length);
        if (bytes.Length < length)
        {
            throw new EndOfStreamException($"Expected to read {length} bytes but only read {bytes.Length}. File might be truncated.");
        }
        return Encoding.ASCII.GetString(bytes).Trim();
    }
}
