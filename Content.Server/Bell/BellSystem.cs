// Taken from https://github.com/emberfall-14/emberfall/pull/4/files with permission

using System.Linq;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.TP14.Bell.Components;
using Content.Shared.TP14.Bell.Systems;
using Content.Shared.Shuttles.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Map.Components;

namespace Content.Server.TP14.Bell.Systems;

public sealed class BellSystem : SharedBellSystem
{
    [Dependency] private readonly BellConsoleSystem _console = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BellComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BellComponent, FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<BellComponent, FTLCompletedEvent>(OnFTLCompleted);

        SubscribeLocalEvent<StationGridAddedEvent>(OnStationGridAdded);
    }

    private void OnMapInit(Entity<BellComponent> ent, ref MapInitEvent args)
    {
        // Find all valid FTL destinations this elevator can reach
        var query = EntityQueryEnumerator<FTLDestinationComponent, MapComponent>();
        while (query.MoveNext(out var mapUid, out var dest, out var map))
        {
            if (!dest.Enabled || _whitelist.IsWhitelistFailOrNull(dest.Whitelist, ent))
                continue;

            ent.Comp.Destinations.Add(new BellDestination
            {
                Name = Name(mapUid),
                Map = map.MapId,
            });
        }
    }

    private void OnFTLStarted(Entity<BellComponent> ent, ref FTLStartedEvent args)
    {
        _console.UpdateConsolesUsing(ent);
    }

    private void OnFTLCompleted(Entity<BellComponent> ent, ref FTLCompletedEvent args)
    {
        _console.UpdateConsolesUsing(ent);
    }

    private void OnStationGridAdded(StationGridAddedEvent args)
    {
        var uid = args.GridId;
        if (!TryComp<BellComponent>(uid, out var comp))
            return;

        // only add the destination once
        if (comp.Station != null)
            return;

        if (_station.GetOwningStation(uid) is not { } station || !TryComp<StationDataComponent>(station, out var data))
            return;

        // add the source station as a destination
        comp.Station = station;
        comp.Destinations.Add(new BellDestination
        {
            Name = Name(station),
            Map = Transform(data.Grids.First()).MapID,
        });
    }
}
