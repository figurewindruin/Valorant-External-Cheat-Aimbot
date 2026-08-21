namespace ValorantCheat.Features;

using ValorantCheat.Core;
using ValorantCheat.Config;
using ValorantCheat.SDK;
using ValorantCheat.Utils;

public sealed class SilentAim
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;
    private readonly CheatConfig _config;

    private const int ViewPitchOffset = 0x04D8;
    private const int ViewYawOffset = 0x04DC;

    public SilentAim(KernelReader reader, OffsetManager offsets, CheatConfig config)
    {
        _reader = reader;
        _offsets = offsets;
        _config = config;
    }

    public void Tick()
    {
        var uworld = new UWorld(_reader, _offsets);
        var localPawn = uworld.GetLocalPawn();
        if (localPawn == IntPtr.Zero) return;

        IntPtr controller = _reader.ReadPointer(localPawn + _offsets.PawnToController);
        if (controller == IntPtr.Zero) return;

        var localPos = _reader.Read<FVector>(localPawn + _offsets.ActorToRootComponent + _offsets.RootComponentToLocation);
        int localTeam = GetTeamId(localPawn);

        IntPtr closestTarget = IntPtr.Zero;
        float closestDist = _config.SilentAimFov;

        var actors = new ActorArray(_reader, _offsets, uworld.LevelAddress);
        foreach (var actor in actors.Enumerate())
        {
            if (actor == localPawn) continue;
            if (GetTeamId(actor) == localTeam) continue;
            if (GetHealth(actor) <= 0) continue;

            var targetPos = _reader.Read<FVector>(actor + _offsets.ActorToRootComponent + _offsets.RootComponentToLocation);
            float dist = Vector3Math.Distance3D(localPos, targetPos);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestTarget = actor;
            }
        }

        if (closestTarget == IntPtr.Zero) return;

        var headPos = GetHeadPosition(closestTarget);
        var (pitch, yaw) = Vector3Math.CalcAngles(localPos, headPos);

        float origPitch = _reader.Read<float>(controller + ViewPitchOffset);
        float origYaw = _reader.Read<float>(controller + ViewYawOffset);

        _reader.Write(controller + ViewPitchOffset, pitch);
        _reader.Write(controller + ViewYawOffset, yaw);

        Thread.Sleep(1);

        _reader.Write(controller + ViewPitchOffset, origPitch);
        _reader.Write(controller + ViewYawOffset, origYaw);
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

    private FVector GetHeadPosition(IntPtr actor)
    {
        IntPtr mesh = _reader.ReadPointer(actor + _offsets.PawnToMesh);
        if (mesh == IntPtr.Zero) return default;

        IntPtr boneArray = _reader.ReadPointer(mesh + _offsets.MeshToBoneArray);
        if (boneArray == IntPtr.Zero) return default;

        var bone = _reader.Read<FTransform>(boneArray + 8 * 0x30);
        var c2w = _reader.Read<FTransform>(mesh + _offsets.MeshToComponentToWorld);
        return FTransform.TransformPosition(c2w, bone.Translation);
    }
}
