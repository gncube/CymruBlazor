using Microsoft.AspNetCore.Components;
using CymruBlazor.Accessibility.Focus;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Accessibility;

public partial class FocusTrap : CymruComponentBase, IAsyncDisposable
{
    [Inject]
    protected IFocusManager FocusManager { get; set; } = default!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Enabled { get; set; } = true;

    [Parameter]
    public bool AutoFocus { get; set; } = true;

    [Parameter]
    public bool RestoreFocus { get; set; } = true;

    protected override string BaseCssClass => "cy-focus-trap";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !Enabled || !AutoFocus)
        {
            return;
        }

        await FocusManager.FocusAsync(
            Id,
            new FocusOptions(
                PreventScroll: true,
                RestorePreviousFocus: RestoreFocus));
    }

    public async ValueTask DisposeAsync()
    {
        if (!RestoreFocus)
        {
            return;
        }

        await FocusManager.RestoreFocusAsync();
    }
}
