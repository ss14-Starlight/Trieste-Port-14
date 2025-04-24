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

namespace Content.Server.StationEvents.Events
{
    public sealed class FlashStormRule : StationEventSystem<FlashStormRuleComponent>
    {

        [Dependency] private readonly GhostSystem _ghost = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;

        
        protected override void Started(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
        {
            base.Started(uid, comp, gameRule, args);

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
              thunder.ThunderRange = 50f;
              thunder.ThunderFrequency = 2f;
            }

            var lights = GetEntityQuery<PoweredLightComponent>();
            foreach (var light in _lookup.GetEntitiesInRange(station, 200f, LookupFlags.StaticSundries ))
            {
                if (!lights.HasComponent(light)) // Flicker lights
                    continue;

                if (!_random.Prob(0.5f))
                    continue;

                _ghost.DoGhostBooEvent(light);
            }

        }

        protected override void Ended(EntityUid uid, FlashStormRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
        {
            base.Ended(uid, comp, gameRule, args);

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
            
        }
    }
}
