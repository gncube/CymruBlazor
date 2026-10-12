using Bunit;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Tests;

/// <summary>
/// Abstract foundational fixture providing framework dependency registration out of the box.
/// </summary>
/// <remarks>
/// This deliberately does not derive from <c>CymruBlazor.Testing.CymruTestContext</c>, the published equivalent for
/// consumers. That context calls <c>AddCymruBlazor()</c> and puts JavaScript interop in loose mode so a consumer's
/// pages render without setup. The library's own tests want the opposite: only the services a test asks for, and strict
/// JavaScript interop, so an unplanned JS call or a missing registration fails the test instead of being absorbed.
/// Deriving from the package would silently weaken about 1,300 tests.
/// </remarks>
public abstract class TestContextBase : BunitContext
{
    protected TestContextBase()
    {
        // Globally register framework utility defaults across all layout tests
        Services.AddSingleton<IComponentIdGenerator, ComponentIdGenerator>();
    }
}
