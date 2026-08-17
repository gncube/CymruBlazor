using Microsoft.AspNetCore.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// Shared fixture for testing <c>CyFormFieldComponentBase&lt;TValue&gt;</c>-derived
/// components, which require a cascaded <see cref="EditContext"/> to
/// render at all (this is <see cref="InputBase{TValue}"/>'s own
/// requirement, not something CymruBlazor adds).
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
    }

    protected static EditContext CreateEditContext(TestFormModel model) =>
        new(model);
}
