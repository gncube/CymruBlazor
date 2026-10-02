using Xunit;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CymruBlazor.Diagnostics;
using CymruBlazor.Extensions;

namespace CymruBlazor.Tests.Diagnostics;

public sealed class DiagnosticsTests
{
    [Fact]
    public void AddCymruBlazor_Defaults_To_Strict()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddCymruBlazor();

        using var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ICyDiagnostics>().IsStrict.ShouldBeTrue();
    }

    [Fact]
    public void AddCymruBlazor_Options_Overload_Configures_Mode()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddCymruBlazor(o => o.Diagnostics = CyDiagnosticsMode.Lenient);

        using var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ICyDiagnostics>().Mode.ShouldBe(CyDiagnosticsMode.Lenient);
    }

    [Fact]
    public void A_Later_Plain_AddCymruBlazor_Does_Not_Reset_Configured_Options()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddCymruBlazor(o => o.Diagnostics = CyDiagnosticsMode.Lenient);
        services.AddCymruBlazor();

        using var sp = services.BuildServiceProvider();

        sp.GetRequiredService<ICyDiagnostics>().Mode.ShouldBe(CyDiagnosticsMode.Lenient);
    }

    [Fact]
    public void Warn_Logs_Each_Distinct_Message_Once()
    {
        var sink = new List<string>();
        var factory = LoggerFactory.Create(b => b.AddProvider(new ListProvider(sink)));
        var diagnostics = new CyDiagnostics(
            new CymruBlazorOptions { Diagnostics = CyDiagnosticsMode.Lenient },
            factory.CreateLogger<CyDiagnostics>());

        diagnostics.Warn("CY9000", "same");
        diagnostics.Warn("CY9000", "same");
        diagnostics.Warn("CY9000", "different");

        sink.Count.ShouldBe(2);
    }

    private sealed class ListProvider(List<string> sink) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new Logger(sink);

        public void Dispose() { }

        private sealed class Logger(List<string> sink) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                sink.Add(formatter(state, exception));
        }
    }
}
