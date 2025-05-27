using System.Runtime.InteropServices;

namespace EDFLibSharp
{
    internal static class NativeMethod
    {

        [DllImport("../edflib", EntryPoint = "edf_open", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr EdfOpen(
            string filepath);

        [DllImport("../edflib", EntryPoint = "edf_close", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int EdfClose(
            IntPtr handle);

        [DllImport("../edflib", EntryPoint = "edf_read_header_info", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int EdfReadHeaderInfo(
            IntPtr handle,
        IntPtr ptr);

        [DllImport("../edflib", EntryPoint = "edf_read_signal_info", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int EdfReadSignalInfo(
            IntPtr handle,
        IntPtr ptr);

        [DllImport("../edflib", EntryPoint = "edf_read_signal_data", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int EdfReadSignalData(
            IntPtr hadnle,
        IntPtr ptr,
        uint signalIndex,
        uint startRecord = 0,
        uint recordCount = 0);

    }
}

