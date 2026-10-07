namespace CymruBlazor.Components.Forms;

/// <summary>
/// Every phrase <see cref="CyCombobox{TValue}"/> and <see cref="CyMultiCombobox{TValue}"/> speaks or labels, as format
/// strings, so a Welsh (or any other) service can replace them. Unset properties keep their English defaults.
/// </summary>
/// <remarks>
/// Pass an instance to the <c>Text</c> parameter:
/// <c>Text="@(new CyComboboxText { NoResults = "Dim canlyniadau" })"</c>.
/// </remarks>
public sealed record CyComboboxText
{
    /// <summary>Status when several options match. {0} count.</summary>
    public string ResultsAvailable { get; init; } = "{0} results available.";

    /// <summary>Status when exactly one option matches.</summary>
    public string OneResult { get; init; } = "1 result available.";

    /// <summary>Status (and visible message) when nothing matches.</summary>
    public string NoResults { get; init; } = "No results found.";

    /// <summary>Status (and visible message) while an async search runs.</summary>
    public string Loading { get; init; } = "Loading results.";

    /// <summary>Status (and visible message) before enough has been typed. {0} the minimum length.</summary>
    public string MinLength { get; init; } = "Type {0} or more characters to see results.";

    /// <summary>Status when more options match than are shown. {0} shown, {1} matching.</summary>
    public string Capped { get; init; } = "Showing the first {0} of {1} results. Keep typing to narrow the list.";

    /// <summary>Status (and visible message) when the async search failed.</summary>
    public string LoadError { get; init; } = "Could not load results. Try again.";

    /// <summary>Status after an option is added to a multiple combobox. {0} option text.</summary>
    public string Selected { get; init; } = "{0} selected.";

    /// <summary>Status after an option is removed from a multiple combobox. {0} option text.</summary>
    public string Removed { get; init; } = "{0} removed.";

    /// <summary>Accessible name of a chip's remove button. {0} option text.</summary>
    public string RemoveItem { get; init; } = "Remove {0}";

    /// <summary>Accessible name of the clear button. {0} the field's label.</summary>
    public string Clear { get; init; } = "Clear {0}";

    /// <summary>Accessible name of the list of chips. {0} the field's label.</summary>
    public string SelectedList { get; init; } = "Selected: {0}";
}
