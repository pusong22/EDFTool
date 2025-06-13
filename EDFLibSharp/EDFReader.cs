using System.Runtime.InteropServices;

namespace EDFLibSharp
{
    public sealed partial class EDFReader : IDisposable
    {
        private readonly struct SignalTransform
        {
            public SignalTransform(double pMax, double pMin, int dMax, int dMin)
            {
                PMin = pMin;
                PMax = pMax;
                DMin = dMin;
                DMax = dMax;
                Unit = (PMax - PMin) / (DMax - DMin);
                Offset = (PMax / Unit - DMax);
            }

            public SignalTransform(SignalInfo info) : this(
                info._physicalMax, info._physicalMin,
                info._digitalMax, info._digitalMin)
            { }

            public double PMin { get; }
            public double PMax { get; }
            public int DMin { get; }
            public int DMax { get; }
            public double Unit { get; }
            public double Offset { get; }
        }

        private IntPtr _handle;
        private bool _disposed = false;

        private SignalTransform[]? _signalTransforms;

        public EDFReader(string filepath)
        {
            _handle = NativeMethod.EdfOpen(filepath);
            if (_handle == IntPtr.Zero)
                throw new Exception("Failed to open EDF file");
        }


        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {

            }

            if (_handle != IntPtr.Zero)
            {
                NativeMethod.EdfClose(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EDFReader() => Dispose(false);


        public HeaderInfo ReadHeader()
        {
            IntPtr headerPtr = IntPtr.Zero;
            try
            {
                headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf<HeaderInfo>());
                int result = NativeMethod.EdfReadHeaderInfo(_handle, headerPtr);
                if (result != 0)
                    throw new Exception($"Header read error: {result}");

                HeaderInfo headerInfo = Marshal.PtrToStructure<HeaderInfo>(headerPtr);

                return headerInfo;
            }
            finally
            {
                if (headerPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(headerPtr);
            }
        }

        public SignalInfo[] ReadSignalInfo(uint signalCount)
        {
            IntPtr signalPtr = IntPtr.Zero;
            try
            {
                signalPtr = Marshal.AllocHGlobal(Marshal.SizeOf<SignalInfo>() * (int)signalCount);
                int result = NativeMethod.EdfReadSignalInfo(_handle, signalPtr);
                if (result != 0)
                    throw new Exception($"Header read error: {result}");

                SignalInfo[] signalInfo = new SignalInfo[signalCount];
                _signalTransforms = new SignalTransform[signalCount];

                for (int i = 0; i < signalInfo.Length; i++)
                {
                    signalInfo[i] = Marshal.PtrToStructure<SignalInfo>(signalPtr + i * Marshal.SizeOf<SignalInfo>());
                    _signalTransforms[i] = new SignalTransform(signalInfo[i]);
                }

                return signalInfo;
            }
            finally
            {
                if (signalPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(signalPtr);
            }
        }


        public void ReadPhysicalData(DataRecord dataRecord)
        {
            if (dataRecord == null)
                throw new ArgumentNullException(nameof(dataRecord));

            ReadDigitalData(dataRecord);
            ToPhysicalVal(dataRecord);
        }

        public void ReadDigitalData(DataRecord dataRecord)
        {
            if (dataRecord == null)
                throw new ArgumentNullException(nameof(dataRecord));

            // sizeof(short) == 2;
            IntPtr ptr = Marshal.AllocHGlobal((int)dataRecord.Length * 2);

            try
            {
                int result = NativeMethod.EdfReadSignalData(
                    _handle, ptr, dataRecord.Index, dataRecord.StartRecord,
                    dataRecord.ReadCount);
                if (result != 0)
                    throw new Exception($"Read signal data error: {result}");

                // 补码
                // foreach (byte b in buf)
                // {
                //     System.Console.WriteLine(Convert.ToString(b, 2));
                // }

                dataRecord.Clear();
                for (int i = 0; i < dataRecord.Length; i++)
                {
                    byte one = Marshal.ReadByte(ptr, 2 * i);
                    byte two = Marshal.ReadByte(ptr, 2 * i + 1);

                    // 小端
                    short raw = (short)((one) | (two << 8));
                    // raw = Marshal.ReadInt16(ptr, 2 * i);

                    dataRecord.Add(raw);
                    //buf[i] = dataInfo.Unit * (raw + dataInfo.Offset);
                    // buf[i] = (raw - dataInfo.DMin) * dataInfo.Unit + dataInfo.PMin;
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        private void ToPhysicalVal(DataRecord dataRecord)
        {
            if (_signalTransforms is null) return;

            var transform = _signalTransforms[dataRecord.Index];
            var buffer = dataRecord.Buffer;
            int count = buffer.Count;
            double dMin = transform.DMin, dMax = transform.DMax;
            double unit = transform.Unit, offset = transform.Offset;
            
            for (int i = 0; i < count; i++)
            {
                double raw = buffer[i];
                if (raw > dMax) raw = dMax;
                if (raw < dMin) raw = dMin;
                buffer[i] = unit * (raw + offset);
            }

            // buf[i] = (raw - dataInfo.DMin) * dataInfo.Unit + dataInfo.PMin;
        }


    }

}
