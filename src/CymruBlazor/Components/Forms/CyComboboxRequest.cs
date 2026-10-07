namespace CymruBlazor.Components.Forms;

/// <summary>
/// What a combobox asks its <c>ItemsProvider</c> for: the options matching the text the user has typed.
/// </summary>
/// <param name="Query">The text typed so far (never <see langword="null"/>; may be empty).</param>
public sealed record CyComboboxRequest(string Query);
