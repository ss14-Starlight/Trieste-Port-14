using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Shared.Gravity
{
    [RegisterComponent]
    public sealed partial class LightningMarkerComponent : Component
    {
        // The lightning prototype this marker should spawn
        [DataField("lightningPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string LightningPrototype = "AdminInstantEffectThunder";

        // The range in which lightning will occur
        [DataField("lightingRange")]
        public float LightningRange = 70f;

        // How often (in seconds) the lightning will occur
        [DataField("lightingFrequency")]
        public float LightningFrequency = 8f;
    }
}
