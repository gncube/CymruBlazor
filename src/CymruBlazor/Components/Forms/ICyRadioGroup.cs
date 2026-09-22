namespace CymruBlazor.Components.Forms;

/// <summary>
/// Non-generic surface a <see cref="CyRadio"/> uses to talk to its enclosing
/// <see cref="CyRadioGroup{TValue}"/>. A <see cref="CyRadio"/> only ever
/// carries a plain string <c>Value</c> - the same shape a native
/// <c>&lt;input type="radio"&gt;</c> has - so it does not need to be
/// generic itself. The one place <c>TValue</c> actually matters (turning
/// the selected string into <c>TValue</c>) stays inside
/// <see cref="CyRadioGroup{TValue}"/>'s own <c>TryParseValueFromString</c>,
/// mirroring how <c>CySelect{TValue}</c>'s <c>&lt;option&gt;</c> values are
/// always strings even though the bound value is not.
/// </summary>
internal interface ICyRadioGroup
{
    /// <summary>The shared <c>name</c> attribute every radio in the group must render, so only one can be checked at a time.</summary>
    string GroupName { get; }

    /// <summary>The currently selected radio's <see cref="CyRadio.Value"/>, or <see langword="null"/> if none is selected.</summary>
    string? SelectedValue { get; }

    /// <summary>Whether the whole group is disabled. A <see cref="CyRadio"/> is also individually disabled when its own <c>Disabled</c> parameter is set.</summary>
    bool Disabled { get; }

    /// <summary>Whether the group currently has a validation error, per the cascaded <c>EditContext</c>.</summary>
    bool HasError { get; }

    /// <summary>Whether the group is marked required.</summary>
    bool Required { get; }

    /// <summary>Called by a <see cref="CyRadio"/> when the user selects it.</summary>
    void Select(string value);
}
