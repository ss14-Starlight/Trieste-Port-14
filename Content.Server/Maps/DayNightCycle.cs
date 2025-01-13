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

  public sealed class DayNightSystem : EntitySystem
{
    private float _updateTimer = 0f;
    private const float updateInterval = 60f;
    private float _transitionProgress = 0f;  // Track the transition progress

    private Color _currentColor;
    private Color _targetColor;

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
            foreach (var planet in EntityManager.EntityQuery<MapLightComponent>())
            {
                EntityUid map = planet.Owner;
                LightCheck(map, frameTime);  // Pass frameTime for smooth transitions
            }
        }
    }

    private void OnCompInit(EntityUid map, MapLightComponent component, ComponentInit args)
    {
        var light = EnsureComp<MapLightComponent>(map);
        _currentColor = Color.FromHex("#D8B059"); // Default to daylight color
        light.AmbientLightColor = _currentColor;

        LightCheck(map, 0f);  // Initialize the lighting for this entity
    }

    private void LightCheck(EntityUid map, float frameTime)
    {
        var light = EnsureComp<MapLightComponent>(map);
        var time = EnsureComp<DayNightComponent>(map);

        // Smoothly transition to the new color
        if (_transitionProgress < 1f)
        {
            _transitionProgress += frameTime * 0.2f; // 0.2f will adjust the speed of the transition
            _transitionProgress = Math.Min(1f, _transitionProgress); // Clamp to 1
            _currentColor = Color.Lerp(_currentColor, _targetColor, _transitionProgress);
            light.AmbientLightColor = _currentColor;
        }

        // Check and trigger transitions based on time of day
        if (_updateTimer >= 500 && !time.IsNoon)
        {
            // Set the target color for the transition
            _targetColor = Color.FromHex("#E6CB8B");  // Midday color
            time.IsNoon = true;
            time.IsDay = false;
            _transitionProgress = 0f;  // Reset progress for smooth transition

            // Optional: Trigger weather conditions (e.g., rain)
            var WillRain = _random.Prob(0.3f);
            var RainStop = _random.Prob(0.3f);
            if (WillRain)
            {
                var weatherPrototypeId = new ProtoId<WeatherPrototype>("Rain");
                var weatherData = new WeatherData
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = null,
                    State = WeatherState.Running
                };
                var rain = EnsureComp<WeatherComponent>(map);
                rain.Weather[weatherPrototypeId] = weatherData;
            }
            else if (RainStop)
            {
                _entityManager.RemoveComponent<WeatherComponent>(map);
            }
        }

        if (_updateTimer >= 700 && !time.IsNight)
        {
            // Set the target color for the transition
            _targetColor = Color.FromHex("#2b3143");  // Night color
            time.IsNight = true;
            time.IsNoon = false;
            _transitionProgress = 0f;  // Reset progress for smooth transition
            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("night-fallen-alert"), time.title, announcementSound: time.Sound, colorOverride: time.Color);

            var RainStop = _random.Prob(0.3f);
            if (RainStop) _entityManager.RemoveComponent<WeatherComponent>(map);
        }

        if (_updateTimer >= 1000 && !time.IsDay)
        {
            // Set the target color for the transition
            _targetColor = Color.FromHex("#D8B059");  // Daylight color
            time.IsDay = true;
            time.IsNight = false;
            _transitionProgress = 0f;  // Reset progress for smooth transition
            _updateTimer = 0f;

            var RainStop = _random.Prob(0.3f);
            if (RainStop) _entityManager.RemoveComponent<WeatherComponent>(map);
        }
    }
}

