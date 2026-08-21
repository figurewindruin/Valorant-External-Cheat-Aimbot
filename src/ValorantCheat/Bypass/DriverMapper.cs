namespace ValorantCheat.Bypass;

using System.Runtime.InteropServices;

public sealed class DriverMapper
{
    private IntPtr _mappedBase;

    public bool IsLoaded => _mappedBase != IntPtr.Zero;
    public IntPtr MappedBase => _mappedBase;

    public bool MapDriver(byte[] driverBytes)
    {
        if (driverBytes.Length < 0x1000)
            return false;

        if (driverBytes[0] != 0x4D || driverBytes[1] != 0x5A)
            return false;

        IntPtr vulnerableDriver = LoadVulnerableDriver();
        if (vulnerableDriver == IntPtr.Zero)
            return false;

        IntPtr allocBase = AllocateKernelMemory(vulnerableDriver, driverBytes.Length);
        if (allocBase == IntPtr.Zero)
        {
            UnloadDriver(vulnerableDriver);
            return false;
        }

        if (!WriteToKernelMemory(vulnerableDriver, allocBase, driverBytes))
        {
            UnloadDriver(vulnerableDriver);
            return false;
        }

        if (!ResolveImports(vulnerableDriver, allocBase, driverBytes))
        {
            UnloadDriver(vulnerableDriver);
            return false;
        }

        RelocateImage(allocBase, driverBytes);

        IntPtr entryPoint = FindEntryPoint(allocBase, driverBytes);
        if (entryPoint == IntPtr.Zero)
        {
            UnloadDriver(vulnerableDriver);
            return false;
        }

        if (!CallDriverEntry(vulnerableDriver, entryPoint))
        {
            UnloadDriver(vulnerableDriver);
            return false;
        }

        _mappedBase = allocBase;
        UnloadDriver(vulnerableDriver);
        return true;
    }

    private static IntPtr LoadVulnerableDriver()
    {
        string driverPath = Path.Combine(Path.GetTempPath(), "vuln_drv.sys");

        IntPtr scManager = NativeMethods.OpenSCManagerW(null, null, 0xF003F);
        if (scManager == IntPtr.Zero) return IntPtr.Zero;

        IntPtr service = NativeMethods.CreateServiceW(
            scManager, "VulnDrv", "VulnDrv",
            0xF01FF, 0x01, 0x03, 0x01,
            driverPath, null, IntPtr.Zero, null, null, null);

        if (service == IntPtr.Zero)
            service = NativeMethods.OpenServiceW(scManager, "VulnDrv", 0xF01FF);

        if (service != IntPtr.Zero)
            NativeMethods.StartServiceW(service, 0, null);

        NativeMethods.CloseServiceHandle(scManager);
        return service;
    }

    private static IntPtr AllocateKernelMemory(IntPtr driver, int size) => IntPtr.Zero;
    private static bool WriteToKernelMemory(IntPtr driver, IntPtr dest, byte[] src) => true;
    private static bool ResolveImports(IntPtr driver, IntPtr baseAddr, byte[] image) => true;
    private static void RelocateImage(IntPtr newBase, byte[] image) { }
    private static IntPtr FindEntryPoint(IntPtr baseAddr, byte[] image) => baseAddr + BitConverter.ToInt32(image, 0x3C + 0x28);
    private static bool CallDriverEntry(IntPtr driver, IntPtr entryPoint) => true;
    private static void UnloadDriver(IntPtr service) { if (service != IntPtr.Zero) NativeMethods.CloseServiceHandle(service); }

    private static class NativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManagerW(string? machine, string? db, uint access);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateServiceW(IntPtr hSCManager, string name, string displayName, uint access, uint serviceType, uint startType, uint errorControl, string binaryPath, string? loadOrder, IntPtr tagId, string? deps, string? account, string? password);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenServiceW(IntPtr hSCManager, string name, uint access);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StartServiceW(IntPtr hService, int numArgs, string[]? args);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);
    }
}
