using Robust.Shared.GameStates;
﻿using Robust.Shared.Audio;

namespace Content.Shared.Light.Components;

/// <summary>
///Used by DayNightSystem.cs
/// </summary>
[RegisterComponent]
public sealed partial class DayNightComponent : Component
{
    [ViewVariables]
    public bool IsDay = true;

    [ViewVariables]
    public bool IsNoon = true;

    [ViewVariables]
    public bool IsNight = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public LocId title = "External Sensor";

    /// <summary>
    /// Announcement color
    /// </summary>
    [ViewVariables]
    [DataField]
    public Color Color = Color.Red;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/alert.ogg");
}
