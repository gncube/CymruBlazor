using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Components.Core;
using CymruBlazor.Enums;
using CymruBlazor.Services;

namespace CymruBlazor.Components.Layout;

/// <summary>
/// Site footer - navy band (matching the NHS Wales/DHCW header+footer
/// colour pairing), optional link groups, optional copyright line.
///
/// Structurally promotes the pattern already used by
/// <c>CymruBlazor.Demo</c>'s <c>DemoFooter.razor</c> into the shipped
/// library - that Demo-only component can be simplified to configure
/// this one instead of maintaining its own markup/CSS, as a follow-up.
/// </summary>
public partial class CyFooter : CyLayoutComponentBase
{
    private string? _resolvedVersion;

    /// <summary>
    /// Resolves <see cref="IPackageVersionService"/> on demand via the
    /// service provider (rather than a required <c>[Inject]</c>
    /// property) so that consuming apps and existing tests that predate
    /// the version feature - and haven't registered the service - are
    /// unaffected. See <see cref="Services.ThemeService"/> for the same
    /// "optional dependency" convention applied to <c>IJSRuntime</c>.
    /// </summary>
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// Optional copyright line, rendered below the link groups.
    /// </summary>
    [Parameter]
    public string? Copyright { get; set; }

    /// <summary>
    /// Controls the footer's background colour. Must be
    /// <see cref="ComponentColour.Primary"/> (the default - the
    /// navy NHS Wales/DHCW header+footer colour pairing),
    /// <see cref="ComponentColour.Secondary"/>,
    /// <see cref="ComponentColour.Surface"/>, or
    /// <see cref="ComponentColour.Neutral"/>. Surface and Neutral
    /// render dark text instead of the light text the navy/secondary
    /// backgrounds need.
    /// </summary>
    [Parameter]
    public ComponentColour Background { get; set; } = ComponentColour.Primary;

    /// <summary>
    /// Explicit version text to display, e.g. <c>"1.2.0"</c>.
    ///
    /// When supplied, this value is always used as-is and no NuGet
    /// lookup is performed. When omitted and <see cref="ShowVersion"/>
    /// is <see langword="true"/>, the version is instead resolved
    /// automatically from the published NuGet package identified by
    /// <see cref="PackageId"/>.
    /// </summary>
    [Parameter]
    public string? Version { get; set; }

    /// <summary>
    /// When <see langword="true"/>, renders a version line - either the
    /// explicit <see cref="Version"/> parameter, or the latest version
    /// published to NuGet for <see cref="PackageId"/>. Defaults to
    /// <see langword="false"/>, since the automatic lookup makes an
    /// outbound HTTP request.
    /// </summary>
    [Parameter]
    public bool ShowVersion { get; set; }

    /// <summary>
    /// The NuGet package id to query for the latest version when
    /// <see cref="ShowVersion"/> is <see langword="true"/> and
    /// <see cref="Version"/> is not supplied. Defaults to
    /// <c>"CymruBlazor"</c>.
    /// </summary>
    [Parameter]
    public string PackageId { get; set; } = "CymruBlazor";

    /// <summary>
    /// Whether the automatic NuGet lookup may return a prerelease
    /// version (e.g. <c>1.0.0-preview.3</c>). Defaults to
    /// <see langword="true"/>, since CymruBlazor itself currently only
    /// publishes prerelease versions - see the README "Status:
    /// Pre-release" note. Has no effect when <see cref="Version"/> is
    /// supplied explicitly.
    /// </summary>
    [Parameter]
    public bool IncludePrerelease { get; set; } = true;

    protected override string BaseCssClass => "cy-footer";

    protected override string BuildCssClass()
    {
        var backgroundSuffix = Background.ToString().ToLowerInvariant();

        return CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass(Class)
            .AddClass($"cy-footer--{backgroundSuffix}")
            .Build();
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (Background is not (ComponentColour.Primary
            or ComponentColour.Secondary
            or ComponentColour.Surface
            or ComponentColour.Neutral))
        {
            throw new InvalidOperationException(
                $"{nameof(CyFooter)}.{nameof(Background)} must be Primary, Secondary, " +
                $"Surface, or Neutral. Received '{Background}'.");
        }
    }

    /// <summary>
    /// The version text actually rendered: the explicit
    /// <see cref="Version"/> parameter when supplied, otherwise the
    /// version resolved from NuGet (which may still be
    /// <see langword="null"/> if the lookup hasn't completed yet, or
    /// failed).
    /// </summary>
    private string? DisplayVersion => Version ?? _resolvedVersion;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!ShowVersion || Version is not null)
        {
            return;
        }

        var packageVersionService = ServiceProvider.GetService<IPackageVersionService>();

        if (packageVersionService is null)
        {
            return;
        }

        _resolvedVersion = await packageVersionService.GetLatestVersionAsync(
            PackageId,
            IncludePrerelease);

        StateHasChanged();
    }
}
