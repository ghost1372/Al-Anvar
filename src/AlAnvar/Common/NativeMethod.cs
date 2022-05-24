using System.Runtime.InteropServices;

namespace AlAnvar.Common;
public static class NativeMethod
{
    [DllImport("NTdll.dll")]
    public static extern int RtlGetVersion(out RTL_OSVERSIONINFOEX lpVersionInformation);

    [StructLayout(LayoutKind.Sequential)]
    public struct RTL_OSVERSIONINFOEX
    {
        public RTL_OSVERSIONINFOEX(uint dwMajorVersion, uint dwMinorVersion, uint dwBuildNumber, uint dwRevision, uint dwPlatformId, string szCSDVersion) : this()
        {
            this.dwMajorVersion = dwMajorVersion;
            this.dwMinorVersion = dwMinorVersion;
            this.dwBuildNumber = dwBuildNumber;
            this.dwRevision = dwRevision;
            this.dwPlatformId = dwPlatformId;
            this.szCSDVersion = szCSDVersion;
        }
        public readonly uint dwOSVersionInfoSize { get; init; } = (uint) Marshal.SizeOf<RTL_OSVERSIONINFOEX>();

        public uint dwMajorVersion;
        public uint dwMinorVersion;
        public uint dwBuildNumber;
        public uint dwRevision;
        public uint dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
    }
}
