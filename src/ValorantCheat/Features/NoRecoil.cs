namespace ValorantCheat.Features;

using ValorantCheat.Core;
using ValorantCheat.Config;
using ValorantCheat.SDK;

public sealed class NoRecoil
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;
    private readonly CheatConfig _config;

    private const int RecoilPitchOffset = 0x04D8;
    private const int RecoilYawOffset = 0x04DC;
    private const int IsFiringOffset = 0x0388;

    private float _prevRecoilPitch;
    private float _prevRecoilYaw;

    public NoRecoil(KernelReader reader, OffsetManager offsets, CheatConfig config)
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

        bool firing = _reader.Read<bool>(localPawn + IsFiringOffset);
        if (!firing)
        {
            _prevRecoilPitch = 0;
            _prevRecoilYaw = 0;
            return;
        }

        float currentPitch = _reader.Read<float>(controller + RecoilPitchOffset);
        float currentYaw = _reader.Read<float>(controller + RecoilYawOffset);

        float deltaPitch = currentPitch - _prevRecoilPitch;
        float deltaYaw = currentYaw - _prevRecoilYaw;

        _prevRecoilPitch = currentPitch;
        _prevRecoilYaw = currentYaw;

        float compensatedPitch = currentPitch - deltaPitch * _config.NoRecoilStrength;
        float compensatedYaw = currentYaw - deltaYaw * _config.NoRecoilStrength;

        _reader.Write(controller + RecoilPitchOffset, compensatedPitch);
        _reader.Write(controller + RecoilYawOffset, compensatedYaw);
    }
}
