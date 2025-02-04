using Content.Server.Shuttles.Components;
using Content.Shared.Gravity;
using Robust.Shared.Map;

namespace Content.Server._TP.Shuttles;

public sealed class ShuttleFallSystem : EntitySystem
{

    private const float UpdateInterval = 5f; // Interval in seconds
    private float _updateTimer = 0f;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShuttleComponent, ComponentInit>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;

        if (_updateTimer >= UpdateInterval)
        {
            // Reset the timer to prevent continual updates.
            _updateTimer = 0f;

            foreach (var entity in EntityManager.EntityQuery<ShuttleComponent>())
            {
                // Get the EntityUid from the ShuttleComponent
                var entityUid = entity.Owner;

                if (!TryComp<TransformComponent>(entityUid, out var transformComponent))
                    continue;

                var currentMap = transformComponent.MapUid;


                if (TryComp<TriesteComponent>(currentMap, out var triesteComponent))
                {
                    Log.Info("iTS ON TRIESTE");
                }
            }
        }
    }


    private void OnInit(Entity<ShuttleComponent> ent, ref ComponentInit args)
    {

    }

}
