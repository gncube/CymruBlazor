using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;
using CymruBlazor.Components.Layout;
using CymruBlazor.Enums;

namespace CymruBlazor.Components.Branding;

/// <summary>
/// A bilingual English/Cymraeg language-switch link, following the
/// Welsh Government/gov.wales convention: the control's visible label is
/// always the *other* language's own name (e.g. shows "Cymraeg" while
/// the page is in English, and "English" while the page is in Welsh) -
/// so it reads correctly to a speaker of either language rather than
/// requiring them to already understand the language they'd be
/// switching away from.
///
/// "Theme aware" follows the same convention as every other component
/// in the library (see <see cref="CyBrandLogo"/> remarks) - it
/// is styled entirely with <c>--cymru-color-*</c> custom properties, so
/// it repaints automatically under <c>[data-theme="dark"]</c> etc.
/// without injecting <c>IThemeService</c> or requiring JS interop.
///
/// This component only renders the control and raises
/// <see cref="CurrentLanguageChanged"/> - like <see cref="CySidebar"/>'s
/// <c>Collapsed</c>/<c>CollapsedChanged</c> pair, it does not itself
/// apply the language switch to page content (e.g. <c>CultureInfo</c>,
/// resource files, routing). Bind <see cref="CurrentLanguage"/> to your
/// own application state to drive that.
/// </summary>
public partial class CyLanguageToggle : CyLayoutComponentBase
{
    private AppLanguage _uncontrolledLanguage;
    private bool _uncontrolledLanguageInitialized;

    /// <summary>
    /// The language currently displayed by the host page. Two-way
    /// bindable via <c>@bind-CurrentLanguage</c>. When left unbound, the
    /// component tracks its own state internally (an "uncontrolled"
    /// toggle), the same fallback pattern <see cref="CyNavigation"/>
    /// uses for its mobile menu open/closed state.
    /// </summary>
    [Parameter]
    public AppLanguage CurrentLanguage { get; set; } = AppLanguage.English;

    [Parameter]
    public EventCallback<AppLanguage> CurrentLanguageChanged { get; set; }

    /// <summary>
    /// When <see langword="true"/>, an icon is rendered before the
    /// label. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the control is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    protected override string BaseCssClass => "cy-language-toggle";

    /// <summary>
    /// The language the control switches *to* when activated - i.e. the
    /// language that is not currently displayed.
    /// </summary>
    private AppLanguage TargetLanguage =>
        EffectiveCurrentLanguage == AppLanguage.English
            ? AppLanguage.Welsh
            : AppLanguage.English;

    /// <summary>
    /// The value actually driving the rendered state: the bound
    /// <see cref="CurrentLanguage"/> when a consumer supplies
    /// <see cref="CurrentLanguageChanged"/>, otherwise the component's
    /// own internally tracked value.
    /// </summary>
    private AppLanguage EffectiveCurrentLanguage =>
        CurrentLanguageChanged.HasDelegate ? CurrentLanguage : _uncontrolledLanguage;

    /// <summary>
    /// The visible label - always the target language's own name, per
    /// the type-level remarks.
    /// </summary>
    private string TargetLanguageLabel =>
        TargetLanguage == AppLanguage.Welsh ? "Cymraeg" : "English";

    /// <summary>
    /// BCP 47 language tag for the target language, applied via the
    /// button's <c>lang</c> attribute so assistive technology/browsers
    /// treat the visible label text ("Cymraeg"/"English") as being in
    /// that language, not the current page language.
    /// </summary>
    private string TargetLanguageTag =>
        TargetLanguage == AppLanguage.Welsh ? "cy" : "en";

    private string ComputedAriaLabel =>
        TargetLanguage == AppLanguage.Welsh
            ? "Newid yr iaith i Gymraeg"
            : "Change the language to English";

    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();

    protected override void OnParametersValidated()
    {
        base.OnParametersValidated();

        // Seed the internal fallback from whatever CurrentLanguage was
        // supplied at first render (e.g. CurrentLanguage="Welsh" with no
        // binding) - only once, since afterwards this component owns
        // the value in the uncontrolled case.
        if (!_uncontrolledLanguageInitialized)
        {
            _uncontrolledLanguage = CurrentLanguage;
            _uncontrolledLanguageInitialized = true;
        }
    }

    private async Task HandleClickAsync()
    {
        if (Disabled)
        {
            return;
        }

        var newLanguage = TargetLanguage;

        if (CurrentLanguageChanged.HasDelegate)
        {
            await CurrentLanguageChanged.InvokeAsync(newLanguage);
        }
        else
        {
            _uncontrolledLanguage = newLanguage;
        }
    }
}
