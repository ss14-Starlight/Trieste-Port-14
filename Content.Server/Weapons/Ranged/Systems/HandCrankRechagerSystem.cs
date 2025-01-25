using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Content.Shared.Audio;
using Robust.Shared.Random;


namespace Content.Server.Weapons.Ranged.Systems;

    public partial class HandCrankRechargerSystem : EntitySystem
    {
        [Dependency] private readonly BatterySystem _battery = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<HandCrankRechargerComponent, GetVerbsEvent<ActivationVerb>>(AddCrankVerb);
            SubscribeLocalEvent<HandCrankRechargerComponent, GunCrankAfterEvent>(OnDoAfter);
        }

        private void AddCrankVerb(EntityUid uid,
            HandCrankRechargerComponent component,
            GetVerbsEvent<ActivationVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract)
                return;

            var user = args.User;

            ActivationVerb verb = new()
            {
                Text = Loc.GetString("toggle-hand-crank-text"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/zap.svg.192dpi.png")),
                Act = () => StartDoAfter(uid, user, component),
                Priority = -1 // For things like PDA's, Open-UI, etc.
            };

            args.Verbs.Add(verb);
        }


        private void StartDoAfter(EntityUid uid, EntityUid user, HandCrankRechargerComponent component)
        {
            var doAfterEventArgs = new DoAfterArgs(EntityManager, user, component.TimeToCrank, new GunCrankAfterEvent(), uid, target: uid, used: uid)
            {
                BreakOnMove = false,
                NeedHand = true,
            };

            if (!TryComp<BatteryComponent>(uid, out var gunBattery))
                return;

            _doAfter.TryStartDoAfter(doAfterEventArgs);
        }


        private void OnDoAfter(EntityUid uid, HandCrankRechargerComponent gun, GunCrankAfterEvent args)
        {

            gun.Cranking = true;

            if (!TryComp<BatteryComponent>(uid, out var gunBattery))
                return;
            _audio.PlayPvs(gun.CrankSound, uid,
                AudioHelpers.WithVariation(0.125f, _random).WithVolume(1f));

            _battery.SetCharge(gunBattery.Owner, gunBattery.CurrentCharge + gun.AmountRecharged, gunBattery);
            gun.Cranking = false;
            args.Repeat = (gunBattery.CurrentCharge < gun.TargetCharge);
            args.Handled = true;
        }

    }
