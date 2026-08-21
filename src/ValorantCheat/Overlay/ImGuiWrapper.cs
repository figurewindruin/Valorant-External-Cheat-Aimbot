namespace ValorantCheat.Overlay;

using ValorantCheat.Config;

public sealed class ImGuiWrapper
{
    private readonly RenderEngine _engine;
    private bool _menuVisible;

    private const uint HeaderColor = 0xFF2222AAu;
    private const uint TextColor = 0xFFFFFFFFu;
    private const uint EnabledColor = 0xFF00FF00u;
    private const uint DisabledColor = 0xFFFF0000u;

    public ImGuiWrapper(RenderEngine engine)
    {
        _engine = engine;
    }

    public void Render(CheatConfig config)
    {
        if (!_menuVisible) return;

        float x = 20, y = 20;
        float width = 280, lineHeight = 22;

        _engine.DrawFilledBox(x, y, width, lineHeight * 14, 0xCC1A1A2Eu);
        _engine.DrawBox(x, y, width, lineHeight * 14, HeaderColor);

        _engine.DrawText("VALORANT EXTERNAL v3.1.0", x + 10, y + 4, HeaderColor);
        y += lineHeight + 4;

        DrawToggle("Aimbot", config.AimbotEnabled, x + 10, ref y, lineHeight);
        DrawToggle("Silent Aim", config.SilentAimEnabled, x + 10, ref y, lineHeight);
        DrawToggle("Triggerbot", config.TriggerBotEnabled, x + 10, ref y, lineHeight);
        DrawToggle("No Recoil", config.NoRecoilEnabled, x + 10, ref y, lineHeight);
        DrawToggle("ESP", config.EspEnabled, x + 10, ref y, lineHeight);
        DrawToggle("ESP Health", config.EspShowHealth, x + 10, ref y, lineHeight);
        DrawToggle("ESP Agent", config.EspShowAgent, x + 10, ref y, lineHeight);
        DrawToggle("ESP Distance", config.EspShowDistance, x + 10, ref y, lineHeight);

        y += 4;
        DrawSlider("Aim FOV", config.AimFov, 1f, 30f, x + 10, ref y, lineHeight);
        DrawSlider("Aim Smooth", config.AimSmooth, 1f, 20f, x + 10, ref y, lineHeight);
        DrawSlider("RCS Strength", config.NoRecoilStrength, 0.1f, 2f, x + 10, ref y, lineHeight);
    }

    public void Toggle() => _menuVisible = !_menuVisible;

    private void DrawToggle(string label, bool value, float x, ref float y, float lineH)
    {
        uint color = value ? EnabledColor : DisabledColor;
        string status = value ? "ON" : "OFF";
        _engine.DrawText($"{label}: [{status}]", x, y, color);
        y += lineH;
    }

    private void DrawSlider(string label, float value, float min, float max, float x, ref float y, float lineH)
    {
        float pct = (value - min) / (max - min);
        _engine.DrawText($"{label}: {value:F1}", x, y, TextColor);
        _engine.DrawFilledBox(x + 180, y + 4, 80, 8, 0xFF444444u);
        _engine.DrawFilledBox(x + 180, y + 4, 80 * pct, 8, HeaderColor);
        y += lineH;
    }
}
