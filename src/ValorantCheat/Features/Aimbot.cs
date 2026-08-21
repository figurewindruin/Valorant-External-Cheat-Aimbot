namespace ValorantCheat.Features;

using ValorantCheat.Core;
using ValorantCheat.Config;
using ValorantCheat.SDK;
using ValorantCheat.Utils;
using System.Runtime.InteropServices;

public sealed class Aimbot
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;
    private readonly CheatConfig _config;

    public Aimbot(KernelReader reader, OffsetManager offsets, CheatConfig config)
    {
        _reader = reader;
        _offsets = offsets;
        _config = config;
    }

    public void Tick()
    {
        if (!IsKeyDown(_config.AimKey)) return;

        var uworld = new UWorld(_reader, _offsets);
        var localPawn = uworld.GetLocalPawn();
        if (localPawn == IntPtr.Zero) return;

        var localTeam = _reader.Read<int>(localPawn + _offsets.PawnToPlayerState + _offsets.PlayerStateToTeamId);
        var localPos = _reader.Read<FVector>(localPawn + _offsets.ActorToRootComponent + _offsets.RootComponentToLocation);

        IntPtr bestTarget = IntPtr.Zero;
        float bestFov = _config.AimFov;

        var actors = new ActorArray(_reader, _offsets, uworld.LevelAddress);
        foreach (var actor in actors.Enumerate())
        {
            if (actor == localPawn) continue;

            int teamId = GetTeamId(actor);
            if (teamId == localTeam) continue;

            float health = GetHealth(actor);
            if (health <= 0) continue;

            var headPos = GetHeadPosition(actor);
            var screenCenter = new FVector(960, 540, 0);
            float fov = Vector3Math.Distance2D(screenCenter, headPos);

            if (fov < bestFov)
            {
                bestFov = fov;
                bestTarget = actor;
            }
        }

        if (bestTarget == IntPtr.Zero) return;

        var targetHead = GetHeadPosition(bestTarget);
        AimAtTarget(targetHead);
    }

    private FVector GetHeadPosition(IntPtr actor)
    {
        IntPtr mesh = _reader.ReadPointer(actor + _offsets.PawnToMesh);
        if (mesh == IntPtr.Zero) return default;

        IntPtr boneArray = _reader.ReadPointer(mesh + _offsets.MeshToBoneArray);
        if (boneArray == IntPtr.Zero) return default;

        var boneMatrix = _reader.Read<FTransform>(boneArray + 8 * 0x30);
        var componentToWorld = _reader.Read<FTransform>(mesh + _offsets.MeshToComponentToWorld);

        return FTransform.TransformPosition(componentToWorld, boneMatrix.Translation);
    }

    private int GetTeamId(IntPtr actor)
    {
        IntPtr playerState = _reader.ReadPointer(actor + _offsets.PawnToPlayerState);
        if (playerState == IntPtr.Zero) return -1;
        return _reader.Read<int>(playerState + _offsets.PlayerStateToTeamId);
    }

    private float GetHealth(IntPtr actor)
    {
        IntPtr damageHandler = _reader.ReadPointer(actor + _offsets.PawnToDamageHandler);
        if (damageHandler == IntPtr.Zero) return 0;
        return _reader.Read<float>(damageHandler + _offsets.DamageHandlerToHealth);
    }

    private void AimAtTarget(FVector targetWorld)
    {
        float smooth = Math.Max(1f, _config.AimSmooth);
        float dx = targetWorld.X / smooth;
        float dy = targetWorld.Y / smooth;

        int moveX = (int)dx;
        int moveY = (int)dy;

        mouse_event(0x0001, moveX, moveY, 0, 0);
    }

    private static bool IsKeyDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
}
