using Content.Shared.EntityList;
using Robust.Shared.Prototypes;

namespace Content.Server._TP.Shuttles;

[RegisterComponent]
public sealed partial class AirFlyingComponent : Component
{
    [DataField]
    public bool IsFlying;
}
