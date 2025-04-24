using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Shared.Gravity
{
    [RegisterComponent]
    public sealed partial class LightningMarkerComponent : Component
    {
        [DataField("lightningPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string LightningPrototype = "AdminInstantEffectThunder";
    }
}
