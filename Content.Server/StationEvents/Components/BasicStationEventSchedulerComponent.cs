using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityTable.EntitySelectors;


namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(BasicStationEventSchedulerSystem))]
public sealed partial class BasicStationEventSchedulerComponent : Component
{
    public const float MinimumTimeUntilFirstEvent = 150;

    /// <summary>
    /// The gamerules that the scheduler can choose from
    /// </summary>
    /// Reminder that though we could do all selection via the EntityTableSelector, we also need to consider various <see cref="StationEventComponent"/> restrictions.
    /// As such, we want to pass a list of acceptable game rules, which are then parsed for restrictions by the <see cref="EventManagerSystem"/>.
    [DataField(required: true)]
    public EntityTableSelector ScheduledGameRules = default!;
}
