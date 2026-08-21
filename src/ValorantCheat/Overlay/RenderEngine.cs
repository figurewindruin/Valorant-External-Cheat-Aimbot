namespace ValorantCheat.Overlay;

using System.Runtime.InteropServices;
using ValorantCheat.Config;
using ValorantCheat.SDK;

public sealed class RenderEngine : IDisposable
{
    private readonly IntPtr _targetWindow;
    private readonly CheatConfig _config;
    private bool _disposed;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private float[] _viewMatrix = new float[16];

    public RenderEngine(IntPtr targetWindow, CheatConfig config)
    {
        _targetWindow = targetWindow;
        _config = config;
        UpdateDimensions();
    }

    public void BeginFrame()
    {
        UpdateDimensions();
    }

    public void EndFrame()
    {
        Thread.Sleep(1);
    }

    public void SetViewMatrix(float[] matrix)
    {
        Array.Copy(matrix, _viewMatrix, 16);
    }

    public bool WorldToScreen(FVector world, out FVector screen)
    {
        screen = default;

        float w = _viewMatrix[12] * world.X + _viewMatrix[13] * world.Y + _viewMatrix[14] * world.Z + _viewMatrix[15];
        if (w < 0.001f) return false;

        float invW = 1f / w;
        float x = _viewMatrix[0] * world.X + _viewMatrix[1] * world.Y + _viewMatrix[2] * world.Z + _viewMatrix[3];
        float y = _viewMatrix[4] * world.X + _viewMatrix[5] * world.Y + _viewMatrix[6] * world.Z + _viewMatrix[7];

        screen = new FVector(
            Width / 2f + (x * invW) * Width / 2f,
            Height / 2f - (y * invW) * Height / 2f,
            0);

        return screen.X >= 0 && screen.X <= Width && screen.Y >= 0 && screen.Y <= Height;
    }

    public void DrawBox(float x, float y, float w, float h, uint color) { }
    public void DrawFilledBox(float x, float y, float w, float h, uint color) { }
    public void DrawLine(float x1, float y1, float x2, float y2, uint color) { }
    public void DrawCircle(float cx, float cy, float radius, uint color) { }
    public void DrawText(string text, float x, float y, uint color) { }

    private void UpdateDimensions()
    {
        if (_targetWindow == IntPtr.Zero) return;
        if (GetWindowRect(_targetWindow, out var rect))
        {
            Width = rect.Right - rect.Left;
            Height = rect.Bottom - rect.Top;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
