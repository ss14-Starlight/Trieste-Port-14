using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Shared.Weather;
using Content.Server._TP;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Chat.Systems;
using Content.Server.Event.Systems;
using Robust.Shared.Timing;
using Content.Shared.TP14.Bell.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Server.Audio;
using Content.Server.Weather;
using Content.Shared.Audio;
using Content.Shared.GameTicking;
using Content.Shared.Gravity;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Serilog;
using Content.Server.Parallax;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Parallax.Biomes.Layers;
using Content.Shared.Parallax.Biomes.Markers;



//Summary
// This system is a simple event that will cause a weather change to occur during the event
// The "TargetWeather" variable will dictate what weather prototype is active during the event
// The "ReturnWeather" variable will dictate what weather it returns to after the event (this is permanent until changed again!)
//Summary
namespace Content.Server.StationEvents.Events
{
    public sealed class WeatherChangeRule : StationEventSystem<WeatherChangeRuleComponent>
    {

        [Dependency] private readonly GhostSystem _ghost = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedGameTicker _gameTicker = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedWeatherSystem _weather = default!;
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;


        protected override void Started(EntityUid uid, WeatherChangeRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, comp, gameRule, args);
           
            foreach (var weather in EntityManager.EntityQuery<WeatherComponent>())
            {
                var target = weather.Owner;
                if (!_prototypeManager.TryIndex<WeatherPrototype>(comp.TargetWeather, out var targetWeather))
                {
                    Log.Error("Weather prototype not found!");
                    return;
                }
                var state = new WeatherState();
                var data = new WeatherData();

                var mapId = Transform(target).MapID;
                var mapUid = Transform(target).MapUid;

                _weather.SetWeather(mapId, targetWeather, TimeSpan.FromMinutes(99999));

                if (comp.Sunlight)
                {
                    EnsureComp<MapGridComponent>(mapUid);
                    EnsureComp<MetaDataComponent>(mapUid);

                    if !TryComp<MetaDataComponent>(uid, out var metadata)
                    {
                        Log.Error("Metadata component not found");
                        return;
                    }
                    
                    var light = EnsureComp<MapLightComponent>(mapUid);
                    light.AmbientLightColor = comp.SunlightColor;
                    
                    Dirty(mapUid, light, metadata);
                }
                
                Log.Info("Weather set");
            }

            if (!comp.Lightning)
            {
                 foreach (var thunder in EntityManager.EntityQuery<LightningMarkerComponent>())
                {
                      thunder.Cleared = true;
                }
            }

        }

        protected override void Ended(EntityUid uid, WeatherChangeRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            base.Ended(uid, comp, gameRule, args);

            foreach (var weather in EntityManager.EntityQuery<WeatherComponent>())
            {
                var target = weather.Owner;
                if (!_prototypeManager.TryIndex<WeatherPrototype>(comp.ReturnWeather, out var returnWeather))
                {
                    Log.Error("Weather prototype not found!");
                    return;
                }
                var state = new WeatherState();
                var data = new WeatherData();

                var mapId = Transform(target).MapID;
                var mapUid = Transform(target).MapUid;

                _weather.SetWeather(mapId, returnWeather, TimeSpan.FromMinutes(99999));
                Log.Info("Weather set");

                if (comp.Sunlight)
                {
                    _entManager.RemoveComponent<MapLightComponent>(mapUid);
                    // Dirty(mapUid, light, metadata);
                }
            }

            if (!comp.Lightning)
            {
                 foreach (var thunder in EntityManager.EntityQuery<LightningMarkerComponent>())
                {
                    thunder.Cleared = false;
                }
            }

        }
    }
}
