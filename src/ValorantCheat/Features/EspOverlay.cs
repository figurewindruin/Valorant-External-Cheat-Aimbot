namespace ValorantCheat.Features;

using ValorantCheat.Core;
using ValorantCheat.Config;
using ValorantCheat.SDK;
using ValorantCheat.Overlay;
using ValorantCheat.Utils;

public sealed class EspOverlay
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;
    private readonly CheatConfig _config;
    private readonly RenderEngine _renderer;

    public EspOverlay(KernelReader reader, OffsetManager offsets, CheatConfig config, RenderEngine renderer)
    {
        _reader = reader;
        _offsets = offsets;
        _config = config;
        _renderer = renderer;
    }

    public void Render(AgentDetector agentDetector)
    {
        var uworld = new UWorld(_reader, _offsets);
        var localPawn = uworld.GetLocalPawn();
        if (localPawn == IntPtr.Zero) return;

        int localTeam = GetTeamId(localPawn);

        var actors = new ActorArray(_reader, _offsets, uworld.LevelAddress);
        foreach (var actor in actors.Enumerate())
        {
            if (actor == localPawn) continue;

            int teamId = GetTeamId(actor);
            if (teamId == localTeam && _config.EspEnemyOnly) continue;

            float health = GetHealth(actor);
            if (health <= 0) continue;

            var rootPos = _reader.Read<FVector>(actor + _offsets.ActorToRootComponent + _offsets.RootComponentToLocation);
            var headPos = GetHeadPosition(actor);

            if (!_renderer.WorldToScreen(headPos, out var screenHead)) continue;
            if (!_renderer.WorldToScreen(rootPos, out var screenFeet)) continue;

            float boxH = Math.Abs(screenFeet.Y - screenHead.Y);
            float boxW = boxH * 0.5f;

            bool isEnemy = teamId != localTeam;
            uint color = isEnemy ? 0xFFFF3333u : 0xFF33FF33u;

            _renderer.DrawBox(screenHead.X - boxW / 2, screenHead.Y, boxW, boxH, color);

            if (_config.EspShowHealth)
                DrawHealthBar(screenHead.X - boxW / 2 - 5, screenHead.Y, boxH, health, 150f);

            if (_config.EspShowAgent)
            {
                string agentName = agentDetector.GetAgentName(actor);
                _renderer.DrawText(agentName, screenHead.X, screenHead.Y - 16, 0xFFFFCC00u);
            }

            if (_config.EspShowDistance)
            {
                var localPos = _reader.Read<FVector>(localPawn + _offsets.ActorToRootComponent + _offsets.RootComponentToLocation);
                float dist = Vector3Math.Distance3D(localPos, rootPos) / 100f;
                _renderer.DrawText($"{dist:F0}m", screenHead.X, screenFeet.Y + 2, 0xFFAAAAAAu);
            }
        }
    }

    private void DrawHealthBar(float x, float y, float height, float health, float maxHealth)
    {
        float pct = Math.Clamp(health / maxHealth, 0f, 1f);
        float filled = height * pct;
        uint color = pct > 0.5f ? 0xFF00FF00u : pct > 0.25f ? 0xFFFFFF00u : 0xFFFF0000u;

        _renderer.DrawFilledBox(x, y, 3, height, 0x80000000u);
        _renderer.DrawFilledBox(x, y + (height - filled), 3, filled, color);
    }

    private int GetTeamId(IntPtr actor)
    {
        IntPtr ps = _reader.ReadPointer(actor + _offsets.PawnToPlayerState);
        return ps != IntPtr.Zero ? _reader.Read<int>(ps + _offsets.PlayerStateToTeamId) : -1;
    }

    private float GetHealth(IntPtr actor)
    {
        IntPtr dh = _reader.ReadPointer(actor + _offsets.PawnToDamageHandler);
        return dh != IntPtr.Zero ? _reader.Read<float>(dh + _offsets.DamageHandlerToHealth) : 0;
    }

    private FVector GetHeadPosition(IntPtr actor)
    {
        IntPtr mesh = _reader.ReadPointer(actor + _offsets.PawnToMesh);
        if (mesh == IntPtr.Zero) return default;
        IntPtr boneArr = _reader.ReadPointer(mesh + _offsets.MeshToBoneArray);
        if (boneArr == IntPtr.Zero) return default;
        var bone = _reader.Read<FTransform>(boneArr + 8 * 0x30);
        var c2w = _reader.Read<FTransform>(mesh + _offsets.MeshToComponentToWorld);
        return FTransform.TransformPosition(c2w, bone.Translation);
    }
}
