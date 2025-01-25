using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Random;

namespace Content.Server.Weapons.Ranged.Systems;

public partial class HandCrankRechargerSystem : SharedHandCrankRechargerSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HandCrankRechargerComponent, GunCrankAfterEvent>(OnDoAfter);
    }

    protected override void StartDoAfter(EntityUid uid, EntityUid user, HandCrankRechargerComponent crank)
    {
        if (!TryComp<BatteryComponent>(uid, out var gunBattery))
            return;

        if (!ShouldCharge(uid, crank, gunBattery))
        {
            _popup.PopupEntity(Loc.GetString("The crank wont budge!"), user, user);
            return;
        }

        var doAfterEventArgs = new DoAfterArgs(EntityManager, user, crank.TimeToCrank, new GunCrankAfterEvent(), uid, target: uid, used: uid)
        {
            BreakOnMove = false,
            NeedHand = true,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnDoAfter(Entity<HandCrankRechargerComponent> ent, ref GunCrankAfterEvent args)
    {
        if (!TryComp<BatteryComponent>(ent, out var gunBattery))
            return;

        if (!ShouldCharge(ent, ent.Comp, gunBattery))
        {
            _popup.PopupEntity(Loc.GetString("charge-is-full"), args.User, args.User);
            return;
        }

        _audio.PlayPvs(ent.Comp.CrankSound, ent, AudioHelpers.WithVariation(0.125f, _random).WithVolume(1f));

        // Set the battery charge to the max of either the battery or the hand crank max.
        var chargeAmount = Math.Clamp(gunBattery.CurrentCharge + ent.Comp.AmountRechargedPerCrank, 0f, GetChargeMax(ent.Comp, gunBattery));
        _battery.SetCharge(ent, chargeAmount, gunBattery);

        // Repeat if the gun isn't fully charged. If there is a maximum charge by crank, use that. Otherwise, charge up to the max of the battery!\
        args.Repeat = ShouldCharge(ent, ent.Comp, gunBattery);
        args.Handled = true;
    }

    private bool ShouldCharge(EntityUid gun, HandCrankRechargerComponent crank, BatteryComponent battery)
    {
        return !(_battery.IsFull(gun, battery) || battery.CurrentCharge >= GetChargeMax(crank, battery));
    }

    private float GetChargeMax(HandCrankRechargerComponent crank, BatteryComponent battery)
    {
        return Math.Min(crank.MaximumChargeByCrank ?? battery.MaxCharge, battery.MaxCharge);
    }
}
