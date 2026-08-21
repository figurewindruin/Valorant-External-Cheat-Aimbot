namespace ValorantCheat.Core;

public sealed class OffsetManager
{
    public long GWorld { get; init; }
    public long GNames { get; init; }
    public long GObjects { get; init; }

    public int UWorldToLevel { get; init; }
    public int LevelToActors { get; init; }
    public int ActorCount { get; init; }
    public int ActorToRootComponent { get; init; }
    public int RootComponentToLocation { get; init; }

    public int MeshToComponentToWorld { get; init; }
    public int MeshToBoneArray { get; init; }
    public int MeshToBoneCount { get; init; }

    public int ControllerToPlayerState { get; init; }
    public int PlayerStateToTeamId { get; init; }
    public int PlayerStateToPawn { get; init; }

    public int PawnToHealth { get; init; }
    public int PawnToMesh { get; init; }
    public int PawnToPlayerState { get; init; }
    public int PawnToController { get; init; }
    public int PawnToDamageHandler { get; init; }
    public int DamageHandlerToHealth { get; init; }

    public int AgentToAbilitySystemComponent { get; init; }
    public int AgentIdOffset { get; init; }

    public static async Task<OffsetManager> ResolveAsync(KernelReader reader)
    {
        await Task.Delay(100);

        return new OffsetManager
        {
            GWorld = 0x68A7A78,
            GNames = 0x6845DC0,
            GObjects = 0x685BED8,

            UWorldToLevel = 0x38,
            LevelToActors = 0xA0,
            ActorCount = 0xA8,
            ActorToRootComponent = 0x198,
            RootComponentToLocation = 0x164,

            MeshToComponentToWorld = 0x250,
            MeshToBoneArray = 0x598,
            MeshToBoneCount = 0x5A0,

            ControllerToPlayerState = 0x2C8,
            PlayerStateToTeamId = 0x1038,
            PlayerStateToPawn = 0x308,

            PawnToHealth = 0x820,
            PawnToMesh = 0x318,
            PawnToPlayerState = 0x2C8,
            PawnToController = 0x2B0,
            PawnToDamageHandler = 0x9B8,
            DamageHandlerToHealth = 0x1B0,

            AgentToAbilitySystemComponent = 0xA20,
            AgentIdOffset = 0x1480
        };
    }
}
