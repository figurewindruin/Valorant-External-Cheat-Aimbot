namespace ValorantCheat.Utils;

using ValorantCheat.SDK;

public static class Vector3Math
{
    private const float Rad2Deg = 180f / MathF.PI;
    private const float Deg2Rad = MathF.PI / 180f;

    public static float Distance3D(FVector a, FVector b)
    {
        return (a - b).Length();
    }

    public static float Distance2D(FVector a, FVector b)
    {
        float dx = a.X - b.X;
        float dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static (float Pitch, float Yaw) CalcAngles(FVector source, FVector destination)
    {
        var delta = destination - source;
        float hyp = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

        float pitch = -MathF.Atan2(delta.Z, hyp) * Rad2Deg;
        float yaw = MathF.Atan2(delta.Y, delta.X) * Rad2Deg;

        return (pitch, yaw);
    }

    public static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    public static FVector Lerp(FVector a, FVector b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new FVector(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t);
    }

    public static FVector PredictPosition(FVector pos, FVector velocity, float deltaTime)
    {
        return pos + velocity * deltaTime;
    }

    public static FVector AngleToForward(float pitch, float yaw)
    {
        float cp = MathF.Cos(pitch * Deg2Rad);
        float sp = MathF.Sin(pitch * Deg2Rad);
        float cy = MathF.Cos(yaw * Deg2Rad);
        float sy = MathF.Sin(yaw * Deg2Rad);

        return new FVector(cp * cy, cp * sy, -sp);
    }
}
