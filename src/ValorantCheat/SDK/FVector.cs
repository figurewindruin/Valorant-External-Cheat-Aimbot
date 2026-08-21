namespace ValorantCheat.SDK;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct FVector
{
    public float X;
    public float Y;
    public float Z;

    public FVector(float x, float y, float z) { X = x; Y = y; Z = z; }

    public static FVector operator +(FVector a, FVector b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static FVector operator -(FVector a, FVector b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static FVector operator *(FVector v, float s) => new(v.X * s, v.Y * s, v.Z * s);

    public readonly float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public readonly float LengthSquared() => X * X + Y * Y + Z * Z;

    public readonly FVector Normalized()
    {
        float len = Length();
        return len > 0 ? new(X / len, Y / len, Z / len) : default;
    }

    public static float Dot(FVector a, FVector b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static FVector Cross(FVector a, FVector b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    public override readonly string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}

[StructLayout(LayoutKind.Sequential)]
public struct FQuat
{
    public float X, Y, Z, W;
}

[StructLayout(LayoutKind.Sequential)]
public struct FTransform
{
    public FQuat Rotation;
    public FVector Translation;
    private readonly float _pad0;
    public FVector Scale3D;
    private readonly float _pad1;

    public static FVector TransformPosition(FTransform transform, FVector position)
    {
        var q = transform.Rotation;
        float x2 = q.X + q.X, y2 = q.Y + q.Y, z2 = q.Z + q.Z;
        float xx2 = q.X * x2, yy2 = q.Y * y2, zz2 = q.Z * z2;
        float xy2 = q.X * y2, xz2 = q.X * z2, yz2 = q.Y * z2;
        float wx2 = q.W * x2, wy2 = q.W * y2, wz2 = q.W * z2;

        FVector result;
        result.X = (1.0f - (yy2 + zz2)) * position.X + (xy2 - wz2) * position.Y + (xz2 + wy2) * position.Z;
        result.Y = (xy2 + wz2) * position.X + (1.0f - (xx2 + zz2)) * position.Y + (yz2 - wx2) * position.Z;
        result.Z = (xz2 - wy2) * position.X + (yz2 + wx2) * position.Y + (1.0f - (xx2 + yy2)) * position.Z;

        result.X = result.X * transform.Scale3D.X + transform.Translation.X;
        result.Y = result.Y * transform.Scale3D.Y + transform.Translation.Y;
        result.Z = result.Z * transform.Scale3D.Z + transform.Translation.Z;

        return result;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct FRotator
{
    public float Pitch;
    public float Yaw;
    public float Roll;

    public FRotator(float pitch, float yaw, float roll) { Pitch = pitch; Yaw = yaw; Roll = roll; }
}
