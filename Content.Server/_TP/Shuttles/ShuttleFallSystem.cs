using Content.Server.Shuttles.Components;
using Content.Shared.Gravity;
using Robust.Shared.Map;
using Content.Server._TP.Shuttles_components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._TP.Shuttles;

// Summary//
// This system allows shuttles to fly in the atmosphere of Trieste if they have a specific atmospheric thruster.
// It checks every 5 seconds for two things: Shuttles that are flying in the air in Trieste airspace, and the state of the shuttle's thrusters.
// It uses these to see whether a shuttle will fall into the Waste Zone and have it's engines waterlogged (offline), or allow it to fly in atmosphere.
// This will allow for things such as a cargo shuttle that navigates from Trieste to some sort of spacial trading station, Caskies invasion crafts, pirate raids, air combat in VTOL fighters, etc.
// Example: If you shoot off the engines of a VTOL shuttle, or EMP it, it will fall from the sky into the ocean surface.
// Summary//
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
                // Get the EntityUid from the ShuttleComponent
                var entityUid = entity.Owner;

                // Make sure that the target isn't a station (we don't want Trieste falling into the ocean... yet)
                if (TryComp<BecomesStationComponent>(entityUid, out var station))
                 {
                     // IT'S A STATION, ABORT ABORT ABORT
                     return;
                 }

                // Get the map the shuttle is currently on
                var currentMap = Transform(entityUid).MapUid;

                // Ensure that the shuttle is in Trieste airspace
                if (TryComp<TriesteComponent>(currentMap, out var triesteComponent))
                {
                    // Does it have atmospheric thrusters on? No? Time to fall! NEAWWWWWWWWW!!!
                    if (TryComp<AirFlyingComponent>(entityUid, out var flight))
                    {
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
                          
                          // The thrusters are waterlogged! Oh no! Guess you better go to Trieste for help...
                          _thruster.DisableLinearThrusters(entityUid);          
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

                 // Get the main thruster component and move to flight checks.
                 if (TryComp<ThrusterComponent>(currentMap, out var thruster))
                 {
                     // Perform flight checks
                     FlightCheck(ThrusterID, thruster, atmoThruster);
                 }
            }
         }
    }


    private void OnInit(Entity<AtmosphericThrusterComponent> ent, ref ComponentInit args)
    {
        var shuttle = Transform(ent).GridUid;

        // Adds the AirFlyingComponent to shuttles with atmospheric thrusters, marking them at in-atmosphere vessels.
        EnsureComp<AirFlyingComponent>(shuttle);

    }

    private void FlightCheck(EntityUid ThrusterID, ThrusterComponent thruster, AtmosphericThrusterComponent atmoThruster)
    {
        // Get the parent shuttle
        var shuttle = Transform(ThrusterID).GridUid;

        // If the thruster is on, yeehaw. The shuttle is flying
        if (thruster.IsOn)
        {
            atmoThruster.Enabled = true;
            atmoThruster.IsFlying = true;
            EnsureComp<AirFlyingComponent>(shuttle);
        }
        else
        {
            // If it's off... Uh-oh. You might be screwed.
            atmoThruster.Enabled = false;
            atmoThruster.IsFlying = false;
            _entityManager.RemoveComponent<AirFlyingComponent>(shuttle);
        }
    }

     private void OnDock(DockEvent args)
    {
        if (TryComp<AirFlyingComponent>(args.GridAUid, out var dockedShip))
        { 
            // If the ship you are docking to is flying, allow safe disablement of atmospheric thrusters.
            if (TryComp<AirFlyingComponent>(args.GridAUid, out var childShip) && dockedShip.IsFlying)
            { 
                childShip.DockedToFlier = true;
            }
        }
   }

      private void OnUndock(UndockEvent args)
    {
         if (TryComp<AirFlyingComponent>(args.GridAUid, out var dockedShip))
        { 
            // When you undock from your parent ship, disables the safety net. Make sure atmospheric thrusters are online before undocking.
            if (TryComp<AirFlyingComponent>(args.GridAUid, out var childShip))
            { 
                childShip.DockedToFlier = false;
            }
        }
        
    }


}
