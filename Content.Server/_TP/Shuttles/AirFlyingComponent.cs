using Content.Shared.EntityList;
using Robust.Shared.Prototypes;

namespace Content.Server._TP.Shuttles_components;

[RegisterComponent]
public sealed partial class AirFlyingComponent : Component
{
    [DataField]
    public bool IsFlying; // Is the shuttle flying?

    [DataField]
    public bool DockedToFlier; // Is it docked to another Flyer?
    
}
