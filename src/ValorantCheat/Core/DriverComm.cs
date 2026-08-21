namespace ValorantCheat.Core;

using System.Runtime.InteropServices;

public sealed class DriverComm : IDisposable
{
    private IntPtr _deviceHandle;
    private bool _disposed;
    private bool _initialized;

    private const string DeviceName = @"\\.\ValorantDriver";
    private const uint IOCTL_READ_MEMORY = 0x80002000;
    private const uint IOCTL_WRITE_MEMORY = 0x80002004;
    private const uint IOCTL_GET_MODULE = 0x80002008;
    private const uint IOCTL_PROTECT_MEMORY = 0x8000200C;

    public bool IsConnected => _initialized && _deviceHandle != IntPtr.Zero;

    public bool Initialize()
    {
        _deviceHandle = CreateFileW(
            DeviceName,
            0xC0000000, // GENERIC_READ | GENERIC_WRITE
            0,
            IntPtr.Zero,
            3, // OPEN_EXISTING
            0,
            IntPtr.Zero);

        _initialized = _deviceHandle != IntPtr.Zero && _deviceHandle != new IntPtr(-1);

        if (!_initialized)
            _deviceHandle = IntPtr.Zero;

        return _initialized;
    }

    public bool ReadMemory(int processId, IntPtr address, byte[] buffer, int size)
    {
        if (!IsConnected) return false;

        var request = new MemoryRequest
        {
            ProcessId = processId,
            Address = address.ToInt64(),
            Buffer = 0,
            Size = size
        };

        byte[] requestBytes = StructToBytes(request);
        return DeviceIoControl(_deviceHandle, IOCTL_READ_MEMORY, requestBytes, requestBytes.Length, buffer, size, out _, IntPtr.Zero);
    }

    public bool WriteMemory(int processId, IntPtr address, byte[] buffer, int size)
    {
        if (!IsConnected) return false;

        var request = new MemoryRequest
        {
            ProcessId = processId,
            Address = address.ToInt64(),
            Buffer = 0,
            Size = size
        };

        byte[] requestBytes = StructToBytes(request);
        byte[] combined = new byte[requestBytes.Length + buffer.Length];
        Buffer.BlockCopy(requestBytes, 0, combined, 0, requestBytes.Length);
        Buffer.BlockCopy(buffer, 0, combined, requestBytes.Length, buffer.Length);

        return DeviceIoControl(_deviceHandle, IOCTL_WRITE_MEMORY, combined, combined.Length, null, 0, out _, IntPtr.Zero);
    }

    public IntPtr GetModuleBase(int processId, string moduleName)
    {
        if (!IsConnected) return IntPtr.Zero;

        byte[] nameBytes = System.Text.Encoding.Unicode.GetBytes(moduleName + '\0');
        byte[] result = new byte[8];

        if (DeviceIoControl(_deviceHandle, IOCTL_GET_MODULE, nameBytes, nameBytes.Length, result, result.Length, out _, IntPtr.Zero))
            return (IntPtr)BitConverter.ToInt64(result, 0);

        return IntPtr.Zero;
    }

    private static byte[] StructToBytes<T>(T structure) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return bytes;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_deviceHandle != IntPtr.Zero)
        {
            CloseHandle(_deviceHandle);
            _deviceHandle = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryRequest
    {
        public int ProcessId;
        public long Address;
        public long Buffer;
        public int Size;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[]? lpInBuffer, int nInBufferSize, byte[]? lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);
}
