using Microsoft.AspNetCore.Components;

namespace CymruBlazor.Tests;

/// <summary>
/// Wraps a child so a test can remove it from the render tree. Blazor disposes a component when its parent stops
/// rendering it, which is the real-world path (a layout or page being replaced); bUnit's own Dispose helpers do not
/// reliably dispose child components synchronously.
/// </summary>
public sealed class RemovableHost : ComponentBase
{
    [Parameter]
    public bool Show { get; set; } = true;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        if (Show)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}
