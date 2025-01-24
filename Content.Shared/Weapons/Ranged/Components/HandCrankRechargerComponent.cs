using System.Numerics;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGunSystem))]
public sealed partial class HandCrankRechargerComponent : Component
{

  [DataField, ViewVariables(VVAccess.ReadWrite)]
  public float AmountRecharged = 20f;

  [DataField, ViewVariables(VVAccess.ReadWrite)]
  public float TimeToCrank = 20f;

}
