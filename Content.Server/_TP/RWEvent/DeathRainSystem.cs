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
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Content.Server.Chat.Systems;
using Content.Shared.Camera;
using System.Numerics;
using Robust.Shared.Player;
using Content.Shared.Weather;


namespace Content.Server._TP.Weather;

public sealed class DeathRainSystem : EntitySystem
{

    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;

    private const float UpdateInterval = 30f;
    private const float RumbleInterval = 15;

    private float _updateTimer = 0f;


    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;

        if (_updateTimer >= RumbleInterval)
        {

          foreach (var rumbler in EntityManager.EntityQuery<RainCrushableComponent>())
            {
                var rumble = rumbler.Owner;
                var kick = new Vector2(_random.NextFloat(), _random.NextFloat()) * 0.5f;
                _sharedCameraRecoil.KickCamera(rumble, kick);
            }    
        }
        
        if (_updateTimer >= UpdateInterval)
        {
            _updateTimer = 0f;

            // in shelter?
            foreach (var entity in EntityManager.EntityQuery<RainCrushableComponent>())
            {
                var shelters = GetEntityQuery<RainShelterComponent>();
                foreach (var shelter in _lookup.GetEntitiesInRange(uid, 0.5f, LookupFlags.StaticSundries ))
                {
                     if (shelters.HasComponent(shelter))
                     {
                         continue;
                     }
                }
                // not in shelter. Bye bye.
                
                // _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("meltdown-alert-warning"), component.title, announcementSound: component.MeltdownSound, colorOverride: component.Color);
                var entityUid = entity.Owner;
                QueueDel(entityUid);
                }
            }
          }
      }
  }
