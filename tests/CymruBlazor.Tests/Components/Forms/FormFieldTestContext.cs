using Microsoft.AspNetCore.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// Shared fixture for testing <c>CyFormFieldComponentBase&lt;TValue&gt;</c>-derived
/// components; the EditContext is optional since 1.8.0 (see StandaloneFieldTests).
/// </summary>
public abstract class FormFieldTestContext : TestContextBase
{
    /// <summary>
    /// A minimal model for building an <see cref="EditContext"/> against
    /// in tests.
    /// </summary>
    protected sealed class TestFormModel
    {
        public string Text { get; set; } = string.Empty;

        public bool Flag { get; set; }

        public string Choice { get; set; } = string.Empty;

        public DateOnly? Date { get; set; }

        public int? Count { get; set; }

        public decimal? Weight { get; set; }

        public IEnumerable<string> Tags { get; set; } = [];
    }

    protected static EditContext CreateEditContext(TestFormModel model) =>
        new(model);
}
