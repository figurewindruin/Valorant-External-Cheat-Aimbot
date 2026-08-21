namespace ValorantCheat.Features;

using ValorantCheat.Core;

public sealed class AgentDetector
{
    private readonly KernelReader _reader;
    private readonly OffsetManager _offsets;

    private static readonly Dictionary<int, string> AgentDatabase = new()
    {
        [1] = "Jett",
        [2] = "Raze",
        [3] = "Breach",
        [4] = "Omen",
        [5] = "Brimstone",
        [6] = "Phoenix",
        [7] = "Sage",
        [8] = "Sova",
        [9] = "Viper",
        [10] = "Cypher",
        [11] = "Reyna",
        [12] = "Killjoy",
        [13] = "Skye",
        [14] = "Yoru",
        [15] = "Astra",
        [16] = "KAY/O",
        [17] = "Chamber",
        [18] = "Neon",
        [19] = "Fade",
        [20] = "Harbor",
        [21] = "Gekko",
        [22] = "Deadlock",
        [23] = "Iso",
        [24] = "Clove",
        [25] = "Vyse",
        [26] = "Tejo",
        [27] = "Waylay"
    };

    public AgentDetector(KernelReader reader, OffsetManager offsets)
    {
        _reader = reader;
        _offsets = offsets;
    }

    public string GetAgentName(IntPtr actorAddress)
    {
        int agentId = _reader.Read<int>(actorAddress + _offsets.AgentIdOffset);
        return AgentDatabase.TryGetValue(agentId, out var name) ? name : $"Agent#{agentId}";
    }

    public int GetAgentId(IntPtr actorAddress)
    {
        return _reader.Read<int>(actorAddress + _offsets.AgentIdOffset);
    }

    public bool IsInitiator(IntPtr actorAddress)
    {
        int id = GetAgentId(actorAddress);
        return id is 3 or 12 or 19 or 22;
    }

    public bool IsDuelist(IntPtr actorAddress)
    {
        int id = GetAgentId(actorAddress);
        return id is 1 or 2 or 6 or 11 or 14 or 18 or 23;
    }

    public bool IsController(IntPtr actorAddress)
    {
        int id = GetAgentId(actorAddress);
        return id is 4 or 5 or 9 or 15 or 20 or 24;
    }

    public bool IsSentinel(IntPtr actorAddress)
    {
        int id = GetAgentId(actorAddress);
        return id is 7 or 8 or 10 or 13 or 17 or 25;
    }
}
