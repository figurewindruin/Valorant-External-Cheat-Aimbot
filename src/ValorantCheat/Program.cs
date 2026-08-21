namespace ValorantCheat;

using ValorantCheat.Core;
using ValorantCheat.Config;
using ValorantCheat.Features;
using ValorantCheat.Overlay;
using ValorantCheat.Bypass;

internal static class Program
{
    private static readonly CancellationTokenSource Cts = new();

    [STAThread]
    static async Task Main(string[] args)
    {
        Console.Title = "Valorant External | v3.1.0";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(Banner);
        Console.ResetColor();

        var config = CheatConfig.Load("valorant_config.json");

        Console.WriteLine("[*] Initializing driver communication...");
        using var driver = new DriverComm();

        if (!driver.Initialize())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] Driver not loaded. Attempting handle hijack fallback...");
            Console.ResetColor();

            var hijack = new HandleHijack();
            if (!hijack.AcquireHandle("VALORANT-Win64-Shipping"))
            {
                Console.WriteLine("[-] Failed to acquire process handle. Exiting.");
                return;
            }
        }

        Console.WriteLine("[+] Waiting for VALORANT...");
        var process = await ValorantProcess.AttachAsync("VALORANT-Win64-Shipping", driver, Cts.Token);
        Console.WriteLine($"[+] Attached to VALORANT | PID: {process.ProcessId}");

        var reader = new KernelReader(driver, process);
        var offsets = await OffsetManager.ResolveAsync(reader);
        Console.WriteLine($"[+] UWorld @ 0x{offsets.GWorld:X} | Offsets resolved");

        var renderEngine = new RenderEngine(process.WindowHandle, config);
        var imgui = new ImGuiWrapper(renderEngine);

        var aimbot = new Aimbot(reader, offsets, config);
        var silentAim = new SilentAim(reader, offsets, config);
        var esp = new EspOverlay(reader, offsets, config, renderEngine);
        var trigger = new TriggerBot(reader, offsets, config);
        var noRecoil = new NoRecoil(reader, offsets, config);
        var agentDetector = new AgentDetector(reader, offsets);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[+] All modules initialized. Press DELETE to toggle menu.");
        Console.ResetColor();

        Console.CancelKeyPress += (_, e) => { e.Cancel = true; Cts.Cancel(); };

        try
        {
            while (!Cts.Token.IsCancellationRequested)
            {
                if (!process.IsRunning) break;

                if (config.AimbotEnabled) aimbot.Tick();
                if (config.SilentAimEnabled) silentAim.Tick();
                if (config.TriggerBotEnabled) trigger.Tick();
                if (config.NoRecoilEnabled) noRecoil.Tick();

                renderEngine.BeginFrame();
                if (config.EspEnabled) esp.Render(agentDetector);
                imgui.Render(config);
                renderEngine.EndFrame();

                await Task.Delay(1, Cts.Token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            renderEngine.Dispose();
            driver.Dispose();
            Console.WriteLine("[*] Cleanup complete.");
        }
    }

    private const string Banner = """

     ██╗   ██╗ █████╗ ██╗          ███████╗██╗  ██╗████████╗
     ██║   ██║██╔══██╗██║          ██╔════╝╚██╗██╔╝╚══██╔══╝
     ██║   ██║███████║██║          █████╗   ╚███╔╝    ██║
     ╚██╗ ██╔╝██╔══██║██║          ██╔══╝   ██╔██╗    ██║
      ╚████╔╝ ██║  ██║███████╗     ███████╗██╔╝ ██╗   ██║
       ╚═══╝  ╚═╝  ╚═╝╚══════╝     ╚══════╝╚═╝  ╚═╝   ╚═╝
                        v3.1.0 | Vanguard Bypass
    """;
}
