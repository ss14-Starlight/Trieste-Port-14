using Content.Server.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.TP.Abyss.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Overlays;
using Robust.Shared.Prototypes;

namespace Content.Server.TP.Abyss.Systems;

/// <summary>
/// Water heavy. Lots of water hurt. Too much water makes person look like one of those hydraulic press videos on instagram.
/// In real terms, this system measures the "depth" of objects, and relates it to their designated crush depths.
/// If you are deeper than your crush depth and don't have an abyssal hardsuit on. Ruh roh.
/// </summary>
public sealed class WaterInteractionSystem : EntitySystem
{
    private const float UpdateTimer = 1f;
    private float _timer = 0f;

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

 public override void Update(float frameTime)
{
    _timer += frameTime;

    if (_timer >= UpdateTimer)
    {
        // Check all objects affected by water
        foreach (var inGas in EntityManager.EntityQuery<InGasComponent>())
        {
            var uid = inGas.Owner;

            if (inGas.InWater)
            {
                if (!TryComp<WaterBlockerComponent >(uid, out var blocker))
                {
                    EnsureComp<WaterViewerComponent>(uid);
                }
            }
            else
            {
                _entityManager.RemoveComponent<WaterViewerComponent>(uid);
            }

            if (TryComp<FlammableComponent >(uid, out var flame) && inGas.InWater)
            {
                if (flame.OnFire)
                {
                    flame.OnFire = false;
                }
            }
            
            // Ignore those wearing abyssal hardsuits
            if (TryComp<AbyssalProtectedComponent>(uid, out var abyssalProtected))
            {
                continue;
            }

            if (inGas.InWater)
            {
                if (inGas.CrushDepth < inGas.WaterAmount)
                {
                    // DIE.
                    var damage = new DamageSpecifier
                    {
                        DamageDict = { ["Blunt"] = 35f }
                    };
                    _damageable.TryChangeDamage(uid, damage, origin: uid);
                }
            }
        }
        _timer = 0f;
    }
}
}
