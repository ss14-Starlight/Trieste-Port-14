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

        
        protected override void Started(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, comp, gameRule, args);

            _sound.DispatchStationEventMusic(uid, "countdown.ogg", StationEventMusicType.Storm); // Play the music

            var bells = GetEntityQuery<BellComponent>();
            foreach (var bell in _lookup.GetEntitiesInRange(station, 200f, LookupFlags.StaticSundries ))
            {
                         if (!bells.HasComponent(bell))
                         continue;

                         bell.CanMove = false; // Prevent bells from being operated
            }

            if (!TryGetRandomStation(out var station))
            {
                return;
            }

            if (!TryComp<WeatherComponent>(station, out var weather))
            {
              Log.Error($"WeatherComponent not found");
              return;
            }

            weather.Weather = comp.StormWeather; // Initiate storm

            foreach (var thunder in EntityManager.EntityQuery<LightningMarkerComponent>())
            {
              thunder.ThunderRange = 50f; // Decrease thunder range
              thunder.ThunderFrequency = 2f; // Increase thunder frequency
            }
            
            comp.Flickering = false;
            while (!comp.Flickering)
            {
                var currentTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
                var startTime = gameRule.ActivatedAt;

                var difference = currentTime.TotalSeconds - startTime.TotalSeconds;

                if (difference.TotalSeconds >= 100)
                {
                    comp.Flickering = true;
                    BeginFlicker(uid, comp, gameRule, station);
                    break;
                }
                else
                {
                    Log.Info($"Not time for flickering. Difference is {difference});
                }
            }

        }
         protected override void BeginFlicker(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, EntityUid station)
         {
             var lights = GetEntityQuery<PoweredLightComponent>();
             while (comp.Flickering)
            {
                 var currentTime = _gameTiming.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
                 var startTime = gameRule.ActivatedAt;
                 var difference = currentTime.TotalSeconds - startTime.TotalSeconds;

                 if (difference.TotalSeconds >= 250)
                {
                    foreach (var light in _lookup.GetEntitiesInRange(station, 200f, LookupFlags.StaticSundries ))
                    {
                         if (!lights.HasComponent(light))
                         continue;

                         light.On = true;
                         comp.Flickering = false;
                         Log.Info("Turning the lights on!");
                    }
                }

                if (difference.TotalSeconds >= 200)
                {
                    foreach (var light in _lookup.GetEntitiesInRange(station, 200f, LookupFlags.StaticSundries ))
                    {
                         if (!lights.HasComponent(light))
                         continue;
                         Log.Info("Turning the lights off!");
                         light.On = false;
                    }
                }
                else
                {
                
                    foreach (var light in _lookup.GetEntitiesInRange(station, 200f, LookupFlags.StaticSundries ))
                    {
                        if (!lights.HasComponent(light)) // Flicker lights
                            continue;

                        if (!_random.Prob(0.5f))
                            continue;
                        
                        _ghost.DoGhostBooEvent(light);
                    }
                }
            }
         
         }
        protected override void Ended(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            base.Ended(uid, comp, gameRule, args);

            var bells = GetEntityQuery<BellComponent>();
            foreach (var bell in _lookup.GetEntitiesInRange(station, 200f, LookupFlags.StaticSundries ))
            {
                         if (!bells.HasComponent(bell))
                         continue;

                         bell.CanMove = true; // Allow bells to move again
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

            if (!TryComp<WeatherComponent>(station, out var weather))
            {
              Log.Error($"WeatherComponent not found");
              return;
            }

            weather.Weather = comp.NormalWeather; // End storm

            comp.Flickering = false;
            
        }
    }
}
