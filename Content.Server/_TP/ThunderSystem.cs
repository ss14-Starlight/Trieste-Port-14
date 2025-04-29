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
using System.Numerics;
using Content.Server.Station.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Content.Server.GameTicking.Rules;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;


namespace Content.Server._TP;
public sealed class ThunderSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;

    public float ThunderInterval = 10f;
    private float _updateTimer = 0f;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        var CustomThunder = CfgManager.GetCVar(CCVars.CustomThunder);

        base.Update(frameTime);

        _updateTimer += frameTime;

        if (_updateTimer >= ThunderInterval)
        {
            foreach (var entity in EntityManager.EntityQuery<LightningMarkerComponent>())
            {
                var entityUid = entity.Owner;
                var transform = Transform(entityUid);
                var coords = transform.Coordinates;

                float ThunderRange;
                float ThunderInterval;

                if (CustomThunder)
                {
                    ThunderRange = CfgManager.GetCVar(CCVars.ThunderRange);
                    ThunderInterval = CfgManager.GetCVar(CCVars.ThunderFrequency);
                }
                else
                {
                    ThunderRange = entity.ThunderRange;
                    ThunderInterval = entity.ThunderFrequency;
                }

                this.ThunderInterval = ThunderInterval;

                Vector2 offset;
                EntityCoordinates newCoords;

                // Keep calculating coordinates until valid ones are found
                do
                {
                     offset = _random.NextVector2(ThunderRange);
                     newCoords = coords.Offset(offset);
                }
              while (_entityManager.EntityQuery<UnderRoofComponent>().Any(marker =>
              Vector2.DistanceSquared(Transform(marker.Owner).Coordinates.Position, newCoords.Position) < 4.5f));
              var LightningType = entity.BaseLightningPrototype // Set default as thunder flash (no strike)
              
              if (entity.StormMode) // If marker is currently in a "Flash Storm"
              {
                  var LightningType = entity.StormThunderPrototype
                  var strikeChance = _random.Prob(0.3f); // Roll a 30% chance for lightning to strike
                  if (strikeChance)
                  {
                      LightningType = entity.StormLightningPrototype; // Change lightning prototype to a strike prototype
                  }
              }

                Spawn(LightningType, newCoords); // Spawn lightning prototype
            }

            _updateTimer = 0; // Reset lightning timer
        }
    }
}
