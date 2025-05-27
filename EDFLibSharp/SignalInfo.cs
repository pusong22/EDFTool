using System.Runtime.InteropServices;

namespace EDFLibSharp
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SignalInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public char[] _label;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public char[] _physicalDim;
        public double _physicalMin;
        public double _physicalMax;
        public int _digitalMin;
        public int _digitalMax;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 81)]
        public char[] _prefiltering;
        public uint _samples;
    }
}


