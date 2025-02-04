using Content.Server.Shuttles.Components;
using Content.Shared.Gravity;
using Robust.Shared.Map;
using Content.Server._TP.Shuttles_components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._TP.Shuttles;

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

                // Get the map the shuttle is currently on
                var currentMap = Transform(entityUid).MapUid;

                // Ensure that the shuttle is in Trieste airspace
                if (TryComp<TriesteComponent>(currentMap, out var triesteComponent))
                {
                    // Does it have atmospheric thrusters? No? Time to fall! NEAWWWWWWWWW!!!
                    if (!TryComp<AirFlyingComponent>(entityUid, out var flight))
                    {
                         Log.Info("It be falling because no engines, SHITE!!");
                         var destination = EntityManager.EntityQuery<FallingDestinationComponent>().FirstOrDefault();
                         if (destination != null)
                        {
                          // Fall the shuttle to the waste zone
                          Transform(entityUid).Coordinates = Transform(destination.Owner).Coordinates;
                          _thruster.DisableLinearThrusters(entityUid); // The thrusters are waterlogged! Oh no!          
                        }
                    }
                }
            }

            foreach (var thruster in EntityManager.EntityQuery<ThrusterComponent>())
            {
                 // Find thrusters
                 var thrusterID = thruster.Owner;

                 // Are those thrusters atmospheric?
                 if (TryComp<AtmosphericThrusterComponent>(currentMap, out var atmoThruster))
                 {
                     FlightCheck(ThrusterID, thruster, atmoThruster);
                 }
            }
         }
    }


    private void OnInit(Entity<AtmosphericThrusterComponent> ent, ref ComponentInit args)
    {
        var shuttle = Transform(ent).GridUid;

        if (ent.Comp.enabled)
        {
            EnsureComp<AirFlyingComponent>(shuttle);
        }
        else
        {
            _entityManager.RemoveComponent<AirFlyingComponent>(shuttle);
        }
    }

    private void FlightCheck(EntityUid ThrusterID, ThrusterComponent thruster, AtmosphericThrusterComponent atmoThruster)
    {
        var shuttle = Transform(ThrusterID).GridUid;

        // If the thruster is on, yeehaw. The shuttle is flying
        if (thruster.IsOn)
        {
            atmoThruster.Enabled = true;
            EnsureComp<AirFlyingComponent>(shuttle);
        }
        else
        {
        // If it's off. Uh-oh.
            atmoThruster.Enabled = false;
            _entityManager.RemoveComponent<AirFlyingComponent>(shuttle);
        }
    }

}
