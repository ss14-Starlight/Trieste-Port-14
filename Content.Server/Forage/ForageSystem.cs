using Content.Server.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Forage;

public sealed class ForageSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ForageComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<ForageComponent> forageable, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        Forage(forageable, args.User);
        args.Handled = true;
    }

    private void Forage(EntityUid foragedUid, EntityUid? forager = null, ForageComponent? component = null)
    {
        if (!Resolve(foragedUid, ref component))
            return;

        if (component.Regrowing)
            return;

        if (component.DestroyOnForage)
            _destructible.DestroyEntity(foragedUid);

        if (component.Loot == null)
            return;

        var pos = _transform.GetMapCoordinates(foragedUid);

        foreach (var (tag, table) in component.Loot)
        {
            if (tag != "All")
            {
                if (forager != null && !_tagSystem.HasTag(forager.Value, tag))
                    continue;
            }

            var getLoot = _proto.Index(table);
            var spawnLoot = getLoot.GetSpawns(_random);
            var spawnPos = pos.Offset(_random.NextVector2(component.GatherOffset));
            Spawn(spawnLoot[0], spawnPos);
        }

        component.Regrowing = true;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ForageComponent>();
        while (query.MoveNext(out _, out var forage))
        {
            if (!forage.Regrowing)
                continue;

            forage.TimeSinceForage += frameTime;

            if (forage.TimeSinceForage > forage.RegrowTime)
            {
                forage.TimeSinceForage = 0.0f;
                forage.Regrowing = false;
            }
        }
    }
}
