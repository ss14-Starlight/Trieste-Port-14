using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;

namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HandCrankRechargerComponent : Component
{

  [DataField, ViewVariables(VVAccess.ReadWrite)]
  public float AmountRecharged = 20f;

  [DataField, ViewVariables(VVAccess.ReadWrite)]
  public float TimeToCrank = 20f;

  [DataField, ViewVariables(VVAccess.ReadWrite)]
  public float TargetCharge = 300f;

  [DataField]
  public bool Cranking = false;

  [DataField("crankSound")]
  public SoundSpecifier? CrankSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Cock/shotgun_close.ogg");

}

[Serializable, NetSerializable]
public sealed partial class GunCrankAfterEvent : SimpleDoAfterEvent
{
}
