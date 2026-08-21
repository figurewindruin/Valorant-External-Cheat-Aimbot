namespace ValorantCheat.SDK;

using ValorantCheat.Core;

public sealed class UWorld
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;

    public IntPtr Address { get; }
    public IntPtr LevelAddress { get; }
    public IntPtr GameInstanceAddress { get; }

    public UWorld(KernelReader reader, OffsetManager offsets)
    {
        _reader = reader;
        _offsets = offsets;

        Address = reader.ReadPointer((IntPtr)offsets.GWorld);
        LevelAddress = reader.ReadPointer(Address + offsets.UWorldToLevel);
        GameInstanceAddress = reader.ReadPointer(Address + 0x1A8);
    }

    public IntPtr GetLocalPawn()
    {
        IntPtr gameInstance = GameInstanceAddress;
        if (gameInstance == IntPtr.Zero) return IntPtr.Zero;

        IntPtr localPlayers = _reader.ReadPointer(gameInstance + 0x40);
        if (localPlayers == IntPtr.Zero) return IntPtr.Zero;

        IntPtr localPlayer = _reader.ReadPointer(localPlayers);
        if (localPlayer == IntPtr.Zero) return IntPtr.Zero;

        IntPtr controller = _reader.ReadPointer(localPlayer + 0x38);
        if (controller == IntPtr.Zero) return IntPtr.Zero;

        IntPtr pawn = _reader.ReadPointer(controller + 0x348);
        return pawn;
    }

    public int GetActorCount()
    {
        if (LevelAddress == IntPtr.Zero) return 0;
        return _reader.Read<int>(LevelAddress + _offsets.ActorCount);
    }
}
