namespace ValorantCheat.SDK;

using ValorantCheat.Core;

public sealed class USkeletalMesh
{
    private readonly KernelReader _reader;
    private readonly IntPtr _meshAddress;
    private readonly OffsetManager _offsets;

    public USkeletalMesh(KernelReader reader, OffsetManager offsets, IntPtr meshAddress)
    {
        _reader = reader;
        _offsets = offsets;
        _meshAddress = meshAddress;
    }

    public bool IsValid => _meshAddress != IntPtr.Zero;

    public FTransform GetComponentToWorld()
    {
        return _reader.Read<FTransform>(_meshAddress + _offsets.MeshToComponentToWorld);
    }

    public int GetBoneCount()
    {
        return _reader.Read<int>(_meshAddress + _offsets.MeshToBoneCount);
    }

    public FVector GetBoneWorldPosition(int boneIndex)
    {
        IntPtr boneArray = _reader.ReadPointer(_meshAddress + _offsets.MeshToBoneArray);
        if (boneArray == IntPtr.Zero) return default;

        var boneTransform = _reader.Read<FTransform>(boneArray + boneIndex * 0x30);
        var c2w = GetComponentToWorld();

        return FTransform.TransformPosition(c2w, boneTransform.Translation);
    }

    public FVector[] GetAllBonePositions()
    {
        int count = GetBoneCount();
        if (count <= 0 || count > 256) return [];

        var positions = new FVector[count];
        var c2w = GetComponentToWorld();
        IntPtr boneArray = _reader.ReadPointer(_meshAddress + _offsets.MeshToBoneArray);
        if (boneArray == IntPtr.Zero) return [];

        byte[] rawData = _reader.ReadBytes(boneArray, count * 0x30);
        for (int i = 0; i < count; i++)
        {
            int offset = i * 0x30;
            var translation = new FVector(
                BitConverter.ToSingle(rawData, offset + 0x10),
                BitConverter.ToSingle(rawData, offset + 0x14),
                BitConverter.ToSingle(rawData, offset + 0x18));

            positions[i] = FTransform.TransformPosition(c2w, translation);
        }

        return positions;
    }

    public static readonly Dictionary<string, int> BoneNames = new()
    {
        ["Head"] = 8,
        ["Neck"] = 7,
        ["Chest"] = 6,
        ["Pelvis"] = 0,
        ["LeftShoulder"] = 11,
        ["RightShoulder"] = 38,
        ["LeftElbow"] = 12,
        ["RightElbow"] = 39,
        ["LeftHand"] = 13,
        ["RightHand"] = 40,
        ["LeftKnee"] = 65,
        ["RightKnee"] = 58,
        ["LeftFoot"] = 67,
        ["RightFoot"] = 60,
    };
}
