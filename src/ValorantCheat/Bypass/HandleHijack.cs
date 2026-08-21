namespace ValorantCheat.Bypass;

using System.Diagnostics;
using System.Runtime.InteropServices;

public sealed class HandleHijack
{
    private IntPtr _hijackedHandle;

    public bool HasHandle => _hijackedHandle != IntPtr.Zero;
    public IntPtr Handle => _hijackedHandle;

    public bool AcquireHandle(string targetProcessName)
    {
        var targetProcs = Process.GetProcessesByName(targetProcessName);
        if (targetProcs.Length == 0) return false;

        int targetPid = targetProcs[0].Id;
        foreach (var p in targetProcs) p.Dispose();

        IntPtr handle = FindExistingHandle(targetPid);
        if (handle != IntPtr.Zero)
        {
            _hijackedHandle = handle;
            return true;
        }

        handle = ExploitLsassHandle(targetPid);
        if (handle != IntPtr.Zero)
        {
            _hijackedHandle = handle;
            return true;
        }

        return false;
    }

    private static IntPtr FindExistingHandle(int targetPid)
    {
        int bufferSize = 0x10000;
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            int status = NtQuerySystemInformation(16, buffer, bufferSize, out int returnLength);

            while (status == unchecked((int)0xC0000004))
            {
                Marshal.FreeHGlobal(buffer);
                bufferSize = returnLength + 0x1000;
                buffer = Marshal.AllocHGlobal(bufferSize);
                status = NtQuerySystemInformation(16, buffer, bufferSize, out returnLength);
            }

            if (status != 0) return IntPtr.Zero;

            int handleCount = Marshal.ReadInt32(buffer);
            IntPtr entryPtr = buffer + 8;

            for (int i = 0; i < Math.Min(handleCount, 100000); i++)
            {
                int ownerPid = Marshal.ReadInt32(entryPtr);
                short handleValue = Marshal.ReadInt16(entryPtr + 4);
                int grantedAccess = Marshal.ReadInt32(entryPtr + 8);

                if (grantedAccess == 0x1FFFFF)
                {
                    IntPtr sourceProcess = OpenProcess(0x0040, false, ownerPid);
                    if (sourceProcess != IntPtr.Zero)
                    {
                        if (DuplicateHandle(sourceProcess, (IntPtr)handleValue,
                            GetCurrentProcess(), out IntPtr duplicated,
                            0x1FFFFF, false, 0))
                        {
                            int dupPid = GetProcessId(duplicated);
                            if (dupPid == targetPid)
                            {
                                CloseHandle(sourceProcess);
                                return duplicated;
                            }
                            CloseHandle(duplicated);
                        }
                        CloseHandle(sourceProcess);
                    }
                }

                entryPtr += 24;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return IntPtr.Zero;
    }

    private static IntPtr ExploitLsassHandle(int targetPid)
    {
        var lsassProcs = Process.GetProcessesByName("lsass");
        if (lsassProcs.Length == 0) return IntPtr.Zero;

        IntPtr lsassHandle = OpenProcess(0x0040, false, lsassProcs[0].Id);
        foreach (var p in lsassProcs) p.Dispose();

        if (lsassHandle == IntPtr.Zero) return IntPtr.Zero;

        try
        {
            IntPtr targetHandle = OpenProcess(0x1FFFFF, false, targetPid);
            if (targetHandle == IntPtr.Zero) return IntPtr.Zero;

            if (DuplicateHandle(GetCurrentProcess(), targetHandle,
                lsassHandle, out IntPtr remoteDup, 0x1FFFFF, false, 0))
            {
                if (DuplicateHandle(lsassHandle, remoteDup,
                    GetCurrentProcess(), out IntPtr finalHandle, 0x1FFFFF, false, 0))
                {
                    return finalHandle;
                }
            }
        }
        finally
        {
            CloseHandle(lsassHandle);
        }

        return IntPtr.Zero;
    }

    [DllImport("ntdll.dll")]
    private static extern int NtQuerySystemInformation(int systemInformationClass, IntPtr systemInformation, int systemInformationLength, out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwOptions);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll")]
    private static extern int GetProcessId(IntPtr process);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);
}
