using Deque.AxeCore.Commons;

namespace CymruBlazor.Testing.Axe;

/// <summary>Thrown by <see cref="CymruAxeAssertions.ShouldHaveNoViolations"/> when an axe-core scan found violations.</summary>
public sealed class CymruAxeViolationException : Exception
{
    /// <summary>Creates the exception with no violations listed.</summary>
    public CymruAxeViolationException()
        : base("The axe-core scan found accessibility violations.")
    {
        Violations = [];
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">The message.</param>
    public CymruAxeViolationException(string message)
        : base(message)
    {
        Violations = [];
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The cause.</param>
    public CymruAxeViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Violations = [];
    }

    /// <summary>Creates the exception for the violations of a scan.</summary>
    /// <param name="violations">The violations found.</param>
    public CymruAxeViolationException(IReadOnlyList<AxeResultItem> violations)
        : base(CymruAxeAssertions.Describe(violations))
    {
        Violations = violations;
    }

    /// <summary>The violations found.</summary>
    public IReadOnlyList<AxeResultItem> Violations { get; }
}
