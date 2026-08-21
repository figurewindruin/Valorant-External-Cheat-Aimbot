namespace ValorantCheat.Core;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed class KernelReader
{
    private readonly DriverComm _driver;
    private readonly ValorantProcess _process;

    public KernelReader(DriverComm driver, ValorantProcess process)
    {
        _driver = driver;
        _process = process;
    }

    public T Read<T>(IntPtr address) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        byte[] buffer = new byte[size];

        if (!_driver.ReadMemory(_process.ProcessId, address, buffer, size))
            return default;

        return MemoryMarshal.Read<T>(buffer);
    }

    public byte[] ReadBytes(IntPtr address, int count)
    {
        byte[] buffer = new byte[count];
        _driver.ReadMemory(_process.ProcessId, address, buffer, count);
        return buffer;
    }

    public string ReadFString(IntPtr address)
    {
        IntPtr dataPtr = (IntPtr)Read<long>(address);
        if (dataPtr == IntPtr.Zero) return string.Empty;

        int length = Read<int>(address + 0x08);
        if (length <= 0 || length > 256) return string.Empty;

        byte[] buffer = ReadBytes(dataPtr, length * 2);
        return System.Text.Encoding.Unicode.GetString(buffer).TrimEnd('\0');
    }

    public bool Write<T>(IntPtr address, T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        byte[] buffer = new byte[size];
        MemoryMarshal.Write(buffer, in value);
        return _driver.WriteMemory(_process.ProcessId, address, buffer, size);
    }

    public IntPtr ReadPointer(IntPtr address) => (IntPtr)Read<long>(address);

    public IntPtr FollowChain(IntPtr baseAddr, params int[] offsets)
    {
        IntPtr current = baseAddr;
        for (int i = 0; i < offsets.Length - 1; i++)
        {
            current = ReadPointer(current + offsets[i]);
            if (current == IntPtr.Zero) return IntPtr.Zero;
        }
        return offsets.Length > 0 ? current + offsets[^1] : current;
    }
}
