using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;


namespace Content.Server.Weapons.Ranged.Systems;
{
    public sealed class HandCrankRechargerSystem : EntitySystem
    {
        [Dependency] private readonly BatterySystem _battery = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<HandCrankRechargerComponent , GetVerbsEvent<ActivationVerb>>(AddCrankVerb);
            SubscribeLocalEvent<HandCrankRechargerComponent, GunCrankAfterEvent>(OnDoAfter);
        }

          private void AddCrankVerb(EntityUid uid, HandCrankRechargerComponent component, GetVerbsEvent<ActivationVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract)
            return;

          ActivationVerb verb = new()
          {
              Text = Loc.GetString("toggle-hand-crank-text"),
              Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/zap.svg.192dpi.png")),
              Act = () => StartDoAfter(uid, component),
              Priority = -1 // For things like PDA's, Open-UI, etc.
           };

            args.Verbs.Add(verb);
         }


           private void StartDoAfter(EntityUid uid, HandCrankRechargerComponent component)
           {
            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, component.TimeToCrank, new GunCrankAfterEvent()));
           }


       private void OnDoAfter(EntityUid uid, HandCrankRechargerComponent gun, GunCrankAfterEvent args)
      {
        if (!TryComp<BatteryComponent>(uid, out var gunBattery))
            return;

        _battery.SetCharge(gunBattery.Owner, gunBattery.CurrentCharge + gun.AmountRecharged, gunBattery);
      }
                                           
    [Serializable, NetSerializable]
    public sealed partial class GunCrankAfterEvent : SimpleDoAfterEvent
    {
    }
}
