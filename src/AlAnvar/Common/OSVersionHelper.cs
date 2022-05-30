namespace AlAnvar.Common;
public static class OSVersionHelper
{
    internal static readonly Version OSVersion = GetOSVersion();

    /// <summary>
    /// Windows NT
    /// </summary>
    public static bool IsWindowsNT { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT;

    /// <summary>
    /// Windows 10 Redstone5 Version 1809 Build 17763 (October 2018 Update)
    /// </summary>
    public static bool IsWindows10_1809 { get; } = IsWindowsNT && OSVersion == new Version(10, 0, 17763);

    /// <summary>
    /// Windows 10 Redstone5 Version 1809 Build 17763 Or Greater (October 2018 Update)
    /// </summary>
    public static bool IsWindows10_1809_OrGreater { get; } = IsWindowsNT && OSVersion >= new Version(10, 0, 17763);

    /// <summary>
    ///     Windows 11 Build 22000
    /// </summary>
    public static bool IsWindows11 { get; } = IsWindowsNT && OSVersion == new Version(10, 0, 22000);

    /// <summary>
    ///     Windows 11 Build 22000 Or Greater
    /// </summary>
    public static bool IsWindows11_OrGreater { get; } = IsWindowsNT && OSVersion >= new Version(10, 0, 22000);

    public static Version GetOSVersion()
    {
        var osv = new NativeMethod.RTL_OSVERSIONINFOEX();
        NativeMethod.RtlGetVersion(out osv);
        return new Version((int) osv.dwMajorVersion, (int) osv.dwMinorVersion, (int) osv.dwBuildNumber, (int) osv.dwRevision);
    }
}
