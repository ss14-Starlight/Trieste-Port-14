using Content.Server.Administration.Logs;
using Content.Server.Atmos.Components;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using JetBrains.Annotations;
using Content.Shared.TP.Abyss.Components;

namespace Content.Server.Atmos.EntitySystems;

/// <summary>
/// Water heavy. Lots of water hurt. Too much water makes person look like one of those hydraulic press videos on instagram.
/// </summary>
public sealed class WaterCrushSystem : EntitySystem
{
    private const float UpdateTimer = 1f;
    private float _timer = 0f;
    [Dependency] private readonly AtmosphereSystem _atmo = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;


 public override void Update(float frameTime)
{
    _timer += frameTime;

    if (_timer < UpdateTimer)
        return;

    _timer -= UpdateTimer;

    var enumerator = EntityQueryEnumerator<InGasComponent, DamageableComponent>();
    while (enumerator.MoveNext(out var uid, out var inGas, out var damageable))
    {
        if (!inGas.DamagedByGas)
        {
            continue;
        }

       if (TryComp<AbyssalProtectedComponent>(uid))
       {
          continue;
       }
        
        if (inGas.InWater && inGas.CrushDepth <= inGas.WaterAmount)
        {
          Log.Info("{uid} is below crush depth. They would die here");
        }
    }
}
}
