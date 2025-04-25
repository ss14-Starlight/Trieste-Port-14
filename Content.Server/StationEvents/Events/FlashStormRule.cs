using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Shared.Weather;
using Content.Server._TP;
using Robust.Shared.Map;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
﻿using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Server.Chat.Systems;
using Content.Server.Event.Systems;
using Robust.Shared.Timing;
using Content.Shared.TP14.Bell.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Server.Audio;
using Content.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;


//Summary
// This code controls "Flash Storms", which are dangerous, violent storms that berate Trieste for about 3 minutes.
// At the start of the storm, lightning increases intensity and nearness to Trieste, and rain picks up into a full Storm.
// It also disables the bell.
// Halfway into the storm, every light on the platform will begin flickering and freaking out, before eventually fully shutting off.
// After this, the storm will begin tapering off, eventually returning to the normal levels of rain and lightning.
//Summary
namespace Content.Server.StationEvents.Events
{
    public sealed class FlashStormRule : StationEventSystem<FlashStormRuleComponent>
    {

        [Dependency] private readonly GhostSystem _ghost = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedGameTicker _gameTicker = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        
        protected override void Started(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, comp, gameRule, args);

            _stormSong = _audio.GetSound(comp.StormMusic);
            _sound.DispatchStationEventMusic(uid, _stormSong, StationEventMusicType.Storm); // Play the music


            if (!TryGetRandomStation(out var station))
            {
                return;
            }

            if (station.HasValue)
            {
                comp.trueStation = station.Value;
            }
            else
            {
                Log.Error("No station foundeth");
            }

            var Map = Transform(comp.trueStation).ParentUid;

            var StationCoords = Transform(comp.trueStation).Coordinates;
            
            foreach (var bell in EntityManager.EntityQuery<BellComponent>())
            {
                if (!EntityManager.HasComponent<BellComponent>(bell.Owner))
                continue;

                // bell.CanMove = false; // Prevent bells from being operated
            }

            if (!TryComp<WeatherComponent>(Map, out var weather))
            {
              Log.Error($"WeatherComponent not found");
              return;
            }

            if (_prototypeManager.TryIndex<WeatherPrototype>(comp.StormWeather, out var stormWeatherProto))
            {
                weather.Weather = stormWeatherProto; // Assign the actual prototype
            }
            else
            {
                Log.Error($"Weather prototype '{comp.StormWeather}' not found.");
            }

            foreach (var thunder in EntityManager.EntityQuery<LightningMarkerComponent>())
            {
              thunder.ThunderRange = 50f; // Decrease thunder range
              thunder.ThunderFrequency = 2f; // Increase thunder frequency
            }
            
            Timer.Spawn(TimeSpan.FromSeconds(60), () =>
            {
                comp.Flickering = false;
            });
        }
         protected override void BeginFlicker(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule)
         {

            var lights = GetEntityQuery<PoweredLightComponent>();
            comp.Flickering = false;
            Timer.Spawn(TimeSpan.FromSeconds(30), () =>
                {
                    comp.Flickering = true;
                }
                else
                {
                    Log.Info($"Not time for flickering. Difference is {difference}");
                });
                
                while (comp.flickering)
                {
                 foreach (var light in _lookup.GetEntitiesInRange(comp.trueStation, 200f, LookupFlags.StaticSundries ))
                    {
                        if (!lights.HasComponent(light)) // Flicker lights
                            continue;

                        if (!_random.Prob(0.5f))
                            continue;
                        
                        _ghost.DoGhostBooEvent(light);
                    }
                }

                Timer.Spawn(TimeSpan.FromSeconds(15), () =>
                {
                    foreach (var light in _lookup.GetEntitiesInRange(comp.trueStation, 200f, LookupFlags.StaticSundries ))
                    {
                         if (!lights.HasComponent(light))
                         continue;
                         Log.Info("Turning the lights off!");
                         light.On = false;
                    }
                 });

                 Timer.Spawn(TimeSpan.FromSeconds(15), () =>
                {

                     foreach (var light in _lookup.GetEntitiesInRange(comp.trueStation, 200f, LookupFlags.StaticSundries ))
                    {
                         if (!lights.HasComponent(light))
                         continue;

                         light.On = true;
                         comp.Flickering = false;
                         Log.Info("Turning the lights on!");
                    }
                });
            }
        }
        protected override void Ended(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            base.Ended(uid, comp, gameRule, args);

            foreach (var bell in EntityManager.EntityQuery<BellComponent>())
            {
            if (!EntityManager.HasComponent<BellComponent>(bell.Owner))
                continue;

            // bell.CanMove = true;
            }

             foreach (var thunder in EntityManager.EntityQuery<LightningMarkerComponent>())
            {
              thunder.ThunderRange = 70f; // Normalize lightning range
              thunder.ThunderFrequency = 8f; // Normalize lightning frequency
            }

            if (!TryGetRandomStation(out var station))
            {
                return;
            }

            var Map = Transform(station).ParentUid;

            if (!TryComp<WeatherComponent>(Map, out var weather))
            {
              Log.Error($"WeatherComponent not found");
              return;
            }

             if (_prototypeManager.TryIndex<WeatherPrototype>(comp.NormalWeather, out var normalWeatherProto))
            {
                weather.Weather = normalWeatherProto;
            }
            else
            {
                Log.Error($"Weather prototype '{comp.NormalWeather}' not found.");
            }

            comp.Flickering = false;
            
        }
    }
}
