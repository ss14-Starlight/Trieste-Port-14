using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;
using Content.Shared.Weather;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;


namespace Content.Server.StationEvents.Components;
{

[RegisterComponent, Access(typeof(FlashStormRule))]
public sealed partial class FlashStormRuleComponent : Component
{
   [DataField("stormWeather", required: true, customTypeSerializer:typeof(PrototypeIdSerializer<WeatherPrototype>))]
   public string StormWeather = string.Empty;
}
}
