using Bunit;
using Microsoft.Extensions.DependencyInjection;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Tests;

/// <summary>
/// Abstract foundational fixture providing framework dependency registration out of the box.
/// </summary>
public abstract class TestContextBase : BunitContext
{
    protected TestContextBase()
    {
        // Globally register framework utility defaults across all layout tests
        Services.AddSingleton<IComponentIdGenerator, ComponentIdGenerator>();
    }
}
