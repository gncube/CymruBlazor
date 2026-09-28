using CymruBlazor.Localisation;
using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Components.Localisation;

/// <summary>
/// Cascades the registered <see cref="ICyLocalizer"/> to every CymruBlazor
/// component beneath it, and re-renders when its language changes. Wrap
/// your layout's body once (the same shape as <c>CyThemeProvider</c>):
/// <code>
/// &lt;CyLocalizationProvider&gt;
///     @Body
/// &lt;/CyLocalizationProvider&gt;
/// </code>
/// Requires <see cref="ICyLocalizer"/> to be registered (via <c>AddCymruBlazor()</c>);
/// pages that don't wrap themselves in this component simply never receive a
/// cascaded value, and every component falls back to its built-in English
/// literal exactly as it did before 1.6.0.
/// </summary>
public partial class CyLocalizationProvider : ComponentBase, IDisposable
{
    [Inject]
    private ICyLocalizer Localizer { get; set; } = default!;

    /// <summary>The content to render beneath the cascaded <see cref="ICyLocalizer"/>.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized() => Localizer.LanguageChanged += HandleLanguageChanged;

    private void HandleLanguageChanged() => InvokeAsync(StateHasChanged);

    /// <inheritdoc />
    public void Dispose()
    {
        Localizer.LanguageChanged -= HandleLanguageChanged;
        GC.SuppressFinalize(this);
    }
}
