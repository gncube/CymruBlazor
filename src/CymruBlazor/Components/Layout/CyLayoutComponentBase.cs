using Microsoft.AspNetCore.Components;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Base class for layout components.
/// </summary>
public abstract class CyLayoutComponentBase : CyComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Implements the base component's hook to construct structural styles seamlessly.
    /// </summary>
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .Build();
}
