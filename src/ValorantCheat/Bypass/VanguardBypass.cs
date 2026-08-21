namespace ValorantCheat.Bypass;

using System.Diagnostics;
using System.Runtime.InteropServices;

public sealed class VanguardBypass
{
    private bool _bypassed;

    public bool IsActive => _bypassed;

    public bool Execute()
    {
        if (!DisableVgkService())
            return false;

        if (!PatchVgcCallbacks())
            return false;

        if (!RemoveKernelCallbacks())
            return false;

        _bypassed = true;
        return true;
    }

    private static bool DisableVgkService()
    {
        try
        {
            var vgkProcess = Process.GetProcessesByName("vgk");
            if (vgkProcess.Length == 0)
                return true;

            IntPtr serviceHandle = OpenSCManagerW(null, null, 0x0001);
            if (serviceHandle == IntPtr.Zero) return false;

            IntPtr vgkService = OpenServiceW(serviceHandle, "vgk", 0x0020);
            if (vgkService == IntPtr.Zero)
            {
                CloseServiceHandle(serviceHandle);
                return false;
            }

            bool result = ControlService(vgkService, 0x00000001, out _);

            CloseServiceHandle(vgkService);
            CloseServiceHandle(serviceHandle);

            return result;
        }
        catch
        {
            return false;
        }
    }

    private static bool PatchVgcCallbacks()
    {
        IntPtr ntoskrnl = GetModuleHandleW("ntoskrnl.exe");
        if (ntoskrnl == IntPtr.Zero) return false;

        IntPtr callbackAddr = GetProcAddress(ntoskrnl, "PsSetCreateProcessNotifyRoutine");
        if (callbackAddr == IntPtr.Zero) return false;

        return true;
    }

    private static bool RemoveKernelCallbacks()
    {
        return true;
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr OpenSCManagerW(string? lpMachineName, string? lpDatabaseName, uint dwDesiredAccess);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr OpenServiceW(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ControlService(IntPtr hService, uint dwControl, out ServiceStatus lpServiceStatus);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseServiceHandle(IntPtr hSCObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandleW(string lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [StructLayout(LayoutKind.Sequential)]
    private struct ServiceStatus
    {
        public int dwServiceType;
        public int dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    }
}
