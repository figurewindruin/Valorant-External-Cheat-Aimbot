namespace ValorantCheat.SDK;

using ValorantCheat.Core;

public sealed class AController
{
    private readonly KernelReader _reader;
    private readonly IntPtr _address;

    private const int ControlRotationOffset = 0x0300;
    private const int PlayerStateOffset = 0x02C8;
    private const int PawnOffset = 0x0348;
    private const int CameraManagerOffset = 0x0348;

    public IntPtr Address => _address;

    public AController(KernelReader reader, IntPtr address)
    {
        _reader = reader;
        _address = address;
    }

    public FRotator GetControlRotation()
    {
        return _reader.Read<FRotator>(_address + ControlRotationOffset);
    }

    public void SetControlRotation(FRotator rotation)
    {
        _reader.Write(_address + ControlRotationOffset, rotation);
    }

    public IntPtr GetPlayerState()
    {
        return _reader.ReadPointer(_address + PlayerStateOffset);
    }

    public IntPtr GetPawn()
    {
        return _reader.ReadPointer(_address + PawnOffset);
    }

    public FVector GetCameraLocation()
    {
        IntPtr cameraManager = _reader.ReadPointer(_address + CameraManagerOffset);
        if (cameraManager == IntPtr.Zero) return default;

        return _reader.Read<FVector>(cameraManager + 0x04D0);
    }

    public FRotator GetCameraRotation()
    {
        IntPtr cameraManager = _reader.ReadPointer(_address + CameraManagerOffset);
        if (cameraManager == IntPtr.Zero) return default;

        return _reader.Read<FRotator>(cameraManager + 0x04DC);
    }
}
