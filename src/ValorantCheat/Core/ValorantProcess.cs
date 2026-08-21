namespace ValorantCheat.Core;

using System.Diagnostics;

public sealed class ValorantProcess
{
    private readonly Process _process;
    private readonly DriverComm _driver;

    public int ProcessId => _process.Id;
    public IntPtr WindowHandle => _process.MainWindowHandle;
    public bool IsRunning => !_process.HasExited;
    public IntPtr BaseAddress { get; }

    private ValorantProcess(Process process, DriverComm driver, IntPtr baseAddress)
    {
        _process = process;
        _driver = driver;
        BaseAddress = baseAddress;
    }

    public static async Task<ValorantProcess> AttachAsync(string processName, DriverComm driver, CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var procs = Process.GetProcessesByName(processName);
            if (procs.Length > 0)
            {
                var proc = procs[0];
                IntPtr baseAddr = proc.MainModule?.BaseAddress ?? IntPtr.Zero;

                for (int i = 1; i < procs.Length; i++) procs[i].Dispose();

                return new ValorantProcess(proc, driver, baseAddr);
            }

            foreach (var p in procs) p.Dispose();
            await Task.Delay(1000, ct);
        }

        throw new OperationCanceledException(ct);
    }

    public ProcessModule? FindModule(string name)
    {
        foreach (ProcessModule mod in _process.Modules)
        {
            if (string.Equals(mod.ModuleName, name, StringComparison.OrdinalIgnoreCase))
                return mod;
        }
        return null;
    }
}
