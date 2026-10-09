using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// Keeps a consumer's value editor from taking down the rule builder (or the circuit). If anything inside throws
/// while rendering, the fallback is shown instead and the exception is logged, never displayed.
/// </summary>
internal sealed class CyRuleEditorBoundary : ErrorBoundaryBase
{
    private static readonly Action<ILogger, Exception?> LogFailure = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(2, "RuleValueEditorFailed"),
        "A CyRuleBuilder value editor threw while rendering and was replaced by its fallback message.");

    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    [Parameter]
    public RenderFragment? Fallback { get; set; }

    protected override Task OnErrorAsync(Exception exception)
    {
        var logger = Services.GetService<ILoggerFactory>()?.CreateLogger("CymruBlazor.CyRuleBuilder");

        if (logger is not null)
        {
            LogFailure(logger, exception);
        }

        return Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (CurrentException is null)
        {
            builder.AddContent(0, ChildContent);
        }
        else
        {
            builder.AddContent(1, Fallback);
        }
    }
}

/// <summary>
/// Renders the consumer's editor fragment from inside its own component, so a throw is attributed to a child of
/// <see cref="CyRuleEditorBoundary"/> (an error boundary only catches errors from its descendants).
/// </summary>
internal sealed class CyRuleEditorHost : ComponentBase
{
    [Parameter, EditorRequired]
    public RenderFragment<CyRuleEditorContext> Editor { get; set; } = default!;

    [Parameter, EditorRequired]
    public CyRuleEditorContext EditorContext { get; set; } = default!;

    protected override void BuildRenderTree(RenderTreeBuilder builder) =>
        builder.AddContent(0, Editor(EditorContext));
}
