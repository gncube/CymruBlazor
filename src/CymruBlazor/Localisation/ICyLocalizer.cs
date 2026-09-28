using CymruBlazor.Enums;

namespace CymruBlazor.Localisation;

/// <summary>
/// A central, cascadable source of the strings CymruBlazor components fall
/// back to when their own override parameter is left unset - the step-2
/// localiser proposed in ADR-0002 and finalised for 1.6.0.
/// </summary>
/// <remarks>
/// <para>
/// <b>Relationship to the step-1 pattern (1.3.0).</b> Every component
/// parameter this service backs is, and remains, a plain <c>string?</c>
/// with an English literal default (e.g.
/// <c>DismissAriaLabel ?? Localizer?.Strings.AlertDismiss ?? "Dismiss"</c>).
/// An explicit value passed to the parameter always wins; <see cref="ICyLocalizer"/>
/// only changes what a component falls back to when no explicit value was
/// given, and only when one has been cascaded to it. A consumer who never
/// registers or cascades an <see cref="ICyLocalizer"/> sees no behaviour
/// change at all - components fall through to the same English literal
/// they always have.
/// </para>
/// <para>
/// <b>Reach.</b> This service intentionally does not cover every
/// localisable parameter in CymruBlazor. <c>CyDateInput.DayLabel</c>/
/// <c>MonthLabel</c>/<c>YearLabel</c>/the four range-error messages, and
/// <c>CyTextArea</c>'s four character-count format strings, are plain
/// (non-nullable) <c>string</c> parameters with a hardcoded default rather
/// than the <c>string? ?? "..."</c> shape - changing their type to
/// accept a localiser fallback would be a breaking API change. A consumer
/// wanting those localised can already bind them directly, e.g.
/// <c>DayLabel="@Localizer.Strings.DateDay"</c> against their own
/// catalogue (see the Demo's <c>AppStrings.Library</c> for the pattern) -
/// no library change is needed for that.
/// </para>
/// <para>
/// <b>Usage.</b> Register the default implementation via
/// <c>AddCymruBlazor()</c>, then wrap the part of the tree that should see
/// it in a <c>CyLocalizationProvider</c>, which cascades the registered
/// <see cref="ICyLocalizer"/> down as a <see cref="Microsoft.AspNetCore.Components.CascadingParameterAttribute"/>.
/// Components declare <c>[CascadingParameter] public ICyLocalizer? Localizer { get; set; }</c>
/// (nullable, so nothing throws for a page that isn't wrapped) and use it
/// only as the last fallback before the English literal.
/// </para>
/// </remarks>
public interface ICyLocalizer
{
    /// <summary>The language <see cref="Strings"/> currently reflects.</summary>
    AppLanguage Language { get; }

    /// <summary>BCP 47 tag for <c>lang</c> attributes (WCAG 3.1.2 Language of Parts).</summary>
    string LangTag { get; }

    /// <summary>The built-in fallback strings for <see cref="Language"/>.</summary>
    CyLocalizedStrings Strings { get; }

    /// <summary>Raised after <see cref="Language"/> changes. A no-op when set to its current value.</summary>
    event Action? LanguageChanged;

    /// <summary>
    /// Sets the active language. Raises <see cref="LanguageChanged"/> once, unless
    /// <paramref name="language"/> is already <see cref="Language"/>, in which case this is a no-op.
    /// </summary>
    void SetLanguage(AppLanguage language);
}
