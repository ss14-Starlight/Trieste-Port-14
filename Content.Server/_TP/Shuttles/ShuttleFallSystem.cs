using Content.Server.Shuttles.Components;
using Content.Shared.Gravity;
using Robust.Shared.Map;
using Content.Server._TP.Shuttles_components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Systems;
using Robust.Shared.Prototypes;
using Content.Server.Shuttles.Events;
using Content.Server.Falling;
using System.Linq;
using Content.Server.Station.Components;

namespace Content.Server._TP.Shuttles;

// Summary//
// This system allows shuttles to fly in the atmosphere of Trieste if they have a specific atmospheric thruster.
// It checks every 5 seconds for two things: Shuttles that are flying in the air in Trieste airspace, and the state of the shuttle's thrusters.
// It uses these to see whether a shuttle will fall into the Waste Zone and have it's engines waterlogged (offline), or allow it to fly in atmosphere.
// This will allow for things such as a cargo shuttle that navigates from Trieste to some sort of orbital trading station, Caskies invasion crafts, pirate raids, air combat in VTOL fighters, etc.
// Example: If you shoot off the engines of a VTOL shuttle or EMP it, it will fall from the sky into the ocean surface.
// Summary//


// TODOS:
// Make the crew onboard the ship get knocked over when the ship falls.
// Make it so docking to Trieste prevents falling.
// Small explosions across shuttle when it crashes.
// Make atmospheric thrusters turn off in space.
// Special sound effect for moving from air to space (muech louder helicoper buzzing)
// Add docks to Trieste to allow these vessels to properly dock.

public sealed class ShuttleFallSystem : EntitySystem
{
    [Dependency] private readonly ThrusterSystem _thruster = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private const float UpdateInterval = 5f; // Interval in seconds
    private float _updateTimer = 0f;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AtmosphericThrusterComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DockEvent>(OnDock);
        SubscribeLocalEvent<UndockEvent>(OnUndock);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;

        if (_updateTimer >= UpdateInterval)
        {
            _updateTimer = 0f;

            foreach (var entity in EntityManager.EntityQuery<ShuttleComponent>())
            {
                Log.Info("Running check");
                // Get the EntityUid from the ShuttleComponent
                var entityUid = entity.Owner;

                // Make sure that the target isn't a station (we don't want Trieste falling into the ocean... yet)
                if (TryComp<TriesteComponent>(entityUid, out var station))
                 {
                     // IT'S A STATION, ABORT ABORT ABORT
                     continue;
                 }

                // Get the map the shuttle is currently on
                var currentMap = Transform(entityUid).MapUid;

                // Ensure that the shuttle is in Trieste airspace
                if (TryComp<TriesteComponent>(currentMap, out var triesteComponent))
                {
                    if (TryComp<AirFlyingComponent>(entity.Owner, out var flight))
                    {
                        Log.Info("Has flying component");
                        // Make sure the ship is actively flying, and is not docked to another flying vessel
                        if (!flight.IsFlying && !flight.DockedToFlier)
                        {

                         // Find where the shuttle will be falling to
                         Log.Info("It be falling, SHITE!!");
                         var destination = EntityManager.EntityQuery<FallingDestinationComponent>().FirstOrDefault();
                             if (destination != null)
                         {
                              // Fall the shuttle to the waste zone
                              Transform(entityUid).Coordinates = Transform(destination.Owner).Coordinates;

                         }
                        }
                    }
                }
            }

            // Get each atmospheric thruster on the map for flight checks
            foreach (var atmoThruster in EntityManager.EntityQuery<AtmosphericThrusterComponent>())
            {
                 // Find the thruster's UID for FlightChecks
                 var thrusterID = atmoThruster.Owner;

                 var currentMap = Transform(thrusterID).MapUid;

                 // Get the main thruster component and move to flight checks.

                     // Perform flight checks
                     FlightCheck(thrusterID, atmoThruster);
            }
         }
    }


    private void OnInit(Entity<AtmosphericThrusterComponent> ent, ref ComponentInit args)
    {
        var shuttle = Transform(ent).GridUid;

        if (shuttle.HasValue)
        {
            // Adds the AirFlyingComponent to shuttles with atmospheric thrusters, marking them at in-atmosphere vessels.
            Log.Info("Added AirFlyingComponent");
            EnsureComp<AirFlyingComponent>(shuttle.Value);
        }
        else
        {
            return;
        }

    }

    private void FlightCheck(EntityUid thrusterID, AtmosphericThrusterComponent atmoThruster)
    {
        // Get the parent shuttle
        var shuttle = Transform(thrusterID).GridUid;

        if (TryComp<ThrusterComponent>(thrusterID, out var thruster))
        {
            // If the thruster is on, yeehaw. The shuttle is flying
            if (thruster.IsOn) // Make sure to link Thrustercomponent to AtmosphericThrusterComponent's state to let the ship know if it's flying or not.
            {
                atmoThruster.Enabled = true;
            }
            else
            {
                // If it's off... Uh-oh. You might be screwed.
                atmoThruster.Enabled = false;
            }
        }
    }

     private void OnDock(DockEvent args)
    {
        if (TryComp<AirFlyingComponent>(args.GridAUid, out var dockedShip))
        {
            // If the ship you are docking to is flying, allow safe disablement of atmospheric thrusters.
            if (TryComp<AirFlyingComponent>(args.GridBUid, out var childShip) && dockedShip.IsFlying)
            {
                Log.Info("Docked to a flying ship");
                childShip.DockedToFlier = true;
            }
        }
   }

      private void OnUndock(UndockEvent args)
    {
         if (TryComp<AirFlyingComponent>(args.GridAUid, out var dockedShip))
        {
            // When you undock from your parent ship, disables the safety net. Make sure atmospheric thrusters are online before undocking.
            if (TryComp<AirFlyingComponent>(args.GridBUid, out var childShip))
            {
                Log.Info("Undocked from a flying ship");
                childShip.DockedToFlier = false;
            }
        }

    }


}
