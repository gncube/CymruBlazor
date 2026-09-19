using Microsoft.AspNetCore.Components.Forms;

namespace CymruBlazor.AccessibilityTests;

/// <summary>
/// Shared fixture for scanning <c>CyFormFieldComponentBase&lt;TValue&gt;</c>-derived
/// components, which require a cascaded <see cref="EditContext"/> to render
/// at all (this is <see cref="InputBase{TValue}"/>'s own requirement, not
/// something CymruBlazor adds). Mirrors
/// CymruBlazor.Tests.Components.Forms.FormFieldTestContext, duplicated here
/// rather than referenced across test projects to keep this project's only
/// dependency on the main library, not on the other test assembly.
/// </summary>
public abstract class FormFieldAxeTestBase : AxeTestBase
{
    protected sealed class TestFormModel
    {
        public string Text { get; set; } = string.Empty;

        public bool Flag { get; set; }

        public string Choice { get; set; } = string.Empty;
    }

    protected static EditContext CreateEditContext(TestFormModel model) => new(model);

    /// <summary>
    /// Creates an <see cref="EditContext"/> whose <paramref name="fieldName"/>
    /// already carries a validation message, so a field rendered against it
    /// shows its error state (<c>aria-invalid</c>, <c>role="alert"</c> text).
    /// </summary>
    protected static EditContext CreateEditContextWithError(TestFormModel model, string fieldName, string message)
    {
        var editContext = new EditContext(model);
        var store = new ValidationMessageStore(editContext);
        store.Add(editContext.Field(fieldName), message);
        editContext.NotifyValidationStateChanged();
        return editContext;
    }
}
