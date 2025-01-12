using Content.Shared.EntityList;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server.Gatherable.Components;

[RegisterComponent]
[Access(typeof(GatherableSystem))]
public sealed partial class ForageComponent : Component
{
    /// <summary>
    ///     Whitelist for specifying the kind of tools can be used to forage
    ///     Supports multiple tags.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist? ToolWhitelist;

    [DataField]
    public Dictionary<string, ProtoId<EntityLootTablePrototype>>? Loot = new();

    /// <summary>
    /// Random shift of the appearing entity during gathering
    /// </summary>
    [DataField]
    public float GatherOffset = 0.3f;

    [DataField]
    public bool DestroyOnGather = false;

    /// <summary>
    /// Time in seconds to regrow produce for gather. Only has an effect if <see cref="DestroyOnGather"/> is true.
    /// </summary>
    [DataField]
    public float RegrowTime = 60.0f;
}
