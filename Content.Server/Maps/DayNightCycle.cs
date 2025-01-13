using Robust.Shared.Prototypes;
using System.Collections.Generic;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Light.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Weather;


namespace Content.Server.Parallax;

public sealed class DayNightSystem : EntitySystem
{
    private float _updateTimer = 0f;
    private const float updateInterval = 60f;

    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MapLightComponent, ComponentInit>(OnCompInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _updateTimer += frameTime;

        if (_updateTimer >= updateInterval)
        {
                 foreach (var planet in EntityManager.EntityQuery<MapLightComponent>)
                {
                    EntityUid map = planet.Owner;
                    LightCheck(map);
                }
        }
          
    }

    private void OnCompInit(EntityUid map, MapLightComponent component, ComponentInit args)
    {
        // Day lighting
        // Daylight: #D8B059
        // Midday: #E6CB8B
        // Moonlight: #2b3143

        var light = EnsureComp<MapLightComponent>(map);
        light.AmbientLightColor = Color.FromHex("#D8B059");

        LightCheck(map);
    }

     private void LightCheck(EntityUid map)
     {
        var light = EnsureComp<MapLightComponent>(map);
        var time = EnsureComp<DayNightComponent>(map);
        
       if (_updateTimer >= 500 && !time.IsNoon)
        {
          light.AmbientLightColor = Color.FromHex("#E6CB8B");
          var WillRain = _random.Prob(0.3f);
          var RainStop = _random.Prob(0.3f);

          time.IsNoon = true;
          time.IsDay = false;

          if (WillRain)
          {
          
              var weatherPrototypeId = new ProtoId<WeatherPrototype>("Rain");
              var weatherData = new WeatherData
              var rain = EnsureComp<WeatherComponent>(map);
              {
                  StartTime = TimeSpan.Zero,
                  EndTime = null,           
                  State = WeatherState.Running
              };
              rain.Weather[weatherPrototypeId] = weatherData;
          }
          else
          {
            if (RainStop) _entityManager.RemoveComponent<WeatherComponent>(map);
          }
}

          
         if (_updateTimer >= 700 && !time.IsNight)
          {
            light.AmbientLightColor = Color.FromHex("#2b3143");
            time.IsNight = true;
            time.IsNoon = false;
            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("night-fallen-alert"), time.title, announcementSound: time.Sound, colorOverride: time.Color);
            var RainStop = _random.Prob(0.3f);
            if (RainStop) _entityManager.RemoveComponent<WeatherComponent>(map);
          }

         if (_updateTimer >= 1000 && !time.IsDay)
          {
            light.AmbientLightColor = Color.FromHex("#2b3143");
            time.IsDay = true;
            time.IsNight = false;
            _updateTimer = 0f;
            var RainStop = _random.Prob(0.3f);
            if (RainStop) _entityManager.RemoveComponent<WeatherComponent>(map);
          }
     
     }

    
