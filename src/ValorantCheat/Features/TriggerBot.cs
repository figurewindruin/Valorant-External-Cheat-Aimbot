namespace ValorantCheat.Features;

using ValorantCheat.Core;
using ValorantCheat.Config;
using ValorantCheat.SDK;
using System.Runtime.InteropServices;

public sealed class TriggerBot
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;
    private readonly CheatConfig _config;
    private readonly Random _rng = new();
    private DateTime _lastShot = DateTime.MinValue;

    public TriggerBot(KernelReader reader, OffsetManager offsets, CheatConfig config)
    {
        _reader = reader;
        _offsets = offsets;
        _config = config;
    }

    public void Tick()
    {
        if (!IsKeyDown(_config.TriggerKey)) return;

        var uworld = new UWorld(_reader, _offsets);
        var localPawn = uworld.GetLocalPawn();
        if (localPawn == IntPtr.Zero) return;

        int localTeam = GetTeamId(localPawn);
        IntPtr controller = _reader.ReadPointer(localPawn + _offsets.PawnToController);
        if (controller == IntPtr.Zero) return;

        IntPtr targetActor = GetCrosshairTarget(controller);
        if (targetActor == IntPtr.Zero) return;

        int targetTeam = GetTeamId(targetActor);
        if (targetTeam == localTeam) return;

        float health = GetHealth(targetActor);
        if (health <= 0) return;

        int delay = _rng.Next(_config.TriggerDelayMin, _config.TriggerDelayMax + 1);
        if ((DateTime.UtcNow - _lastShot).TotalMilliseconds < delay) return;

        mouse_event(0x0002, 0, 0, 0, 0); // MOUSEEVENTF_LEFTDOWN
        Thread.Sleep(_rng.Next(15, 40));
        mouse_event(0x0004, 0, 0, 0, 0); // MOUSEEVENTF_LEFTUP

        _lastShot = DateTime.UtcNow;
    }

    private IntPtr GetCrosshairTarget(IntPtr controller)
    {
        IntPtr ackPawn = _reader.ReadPointer(controller + 0x0458);
        return ackPawn;
    }

    private int GetTeamId(IntPtr actor)
    {
        IntPtr ps = _reader.ReadPointer(actor + _offsets.PawnToPlayerState);
        return ps != IntPtr.Zero ? _reader.Read<int>(ps + _offsets.PlayerStateToTeamId) : -1;
    }

    private float GetHealth(IntPtr actor)
    {
        IntPtr dh = _reader.ReadPointer(actor + _offsets.PawnToDamageHandler);
        return dh != IntPtr.Zero ? _reader.Read<float>(dh + _offsets.DamageHandlerToHealth) : 0;
    }

    private static bool IsKeyDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
}
