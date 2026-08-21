namespace ValorantCheat.Config;

using System.Text.Json;

public sealed class CheatConfig
{
    public bool AimbotEnabled { get; set; } = true;
    public int AimKey { get; set; } = 0x02;
    public float AimFov { get; set; } = 8.0f;
    public float AimSmooth { get; set; } = 4.0f;

    public bool SilentAimEnabled { get; set; }
    public float SilentAimFov { get; set; } = 15.0f;

    public bool TriggerBotEnabled { get; set; } = true;
    public int TriggerKey { get; set; } = 0x06;
    public int TriggerDelayMin { get; set; } = 40;
    public int TriggerDelayMax { get; set; } = 100;

    public bool NoRecoilEnabled { get; set; } = true;
    public float NoRecoilStrength { get; set; } = 0.85f;

    public bool EspEnabled { get; set; } = true;
    public bool EspShowHealth { get; set; } = true;
    public bool EspShowAgent { get; set; } = true;
    public bool EspShowDistance { get; set; } = true;
    public bool EspEnemyOnly { get; set; } = true;

    public int MenuKey { get; set; } = 0x2E; // VK_DELETE
    public int PanicKey { get; set; } = 0x23; // VK_END

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static CheatConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            var defaults = new CheatConfig();
            defaults.Save(path);
            return defaults;
        }

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<CheatConfig>(json, JsonOptions) ?? new CheatConfig();
    }

    public void Save(string path)
    {
        string json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(path, json);
    }
}
