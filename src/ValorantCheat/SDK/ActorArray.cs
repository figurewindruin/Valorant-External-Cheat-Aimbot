namespace ValorantCheat.SDK;

using ValorantCheat.Core;

public sealed class ActorArray
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;
    private readonly IntPtr _levelAddress;

    public ActorArray(KernelReader reader, OffsetManager offsets, IntPtr levelAddress)
    {
        _reader = reader;
        _offsets = offsets;
        _levelAddress = levelAddress;
    }

    public IEnumerable<IntPtr> Enumerate()
    {
        if (_levelAddress == IntPtr.Zero) yield break;

        IntPtr actorArray = _reader.ReadPointer(_levelAddress + _offsets.LevelToActors);
        int actorCount = _reader.Read<int>(_levelAddress + _offsets.ActorCount);

        if (actorArray == IntPtr.Zero || actorCount <= 0) yield break;

        actorCount = Math.Min(actorCount, 1024);

        byte[] buffer = _reader.ReadBytes(actorArray, actorCount * 8);
        for (int i = 0; i < actorCount; i++)
        {
            long addr = BitConverter.ToInt64(buffer, i * 8);
            if (addr != 0)
                yield return (IntPtr)addr;
        }
    }

    public IntPtr GetActorAt(int index)
    {
        IntPtr actorArray = _reader.ReadPointer(_levelAddress + _offsets.LevelToActors);
        if (actorArray == IntPtr.Zero) return IntPtr.Zero;

        return _reader.ReadPointer(actorArray + index * 8);
    }
}
