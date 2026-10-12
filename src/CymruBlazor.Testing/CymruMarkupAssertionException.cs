namespace CymruBlazor.Testing;

/// <summary>Thrown by <see cref="CymruMarkupChecks"/> when rendered markup has structural accessibility problems.</summary>
public sealed class CymruMarkupAssertionException : Exception
{
    /// <summary>Creates the exception with no problems listed.</summary>
    public CymruMarkupAssertionException()
        : base("The markup has accessibility problems.")
    {
        Problems = [];
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">The message.</param>
    public CymruMarkupAssertionException(string message)
        : base(message)
    {
        Problems = [];
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The cause.</param>
    public CymruMarkupAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
        Problems = [];
    }

    /// <summary>Creates the exception for a list of problems.</summary>
    /// <param name="problems">The problems found, one message each.</param>
    public CymruMarkupAssertionException(IReadOnlyList<string> problems)
        : base(BuildMessage(problems))
    {
        Problems = problems;
    }

    /// <summary>The problems found, one message each.</summary>
    public IReadOnlyList<string> Problems { get; }

    private static string BuildMessage(IReadOnlyList<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);

        return FormattableString.Invariant($"The markup has {problems.Count} accessibility problem(s):{Environment.NewLine}")
            + string.Join(Environment.NewLine, problems.Select(problem => $"  - {problem}"));
    }
}
