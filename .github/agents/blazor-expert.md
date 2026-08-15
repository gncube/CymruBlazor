---
title: Blazor Expert Agent
description: Specialized guidance for Blazor WASM component design, state management, and performance optimization
responsibilities:
  - Design and review Blazor components for correctness and performance
  - Recommend state management patterns and component composition
  - Guide async operations, interop, and event handling
  - Evaluate testing strategies for Blazor components
requires:
  - .github/agents/architect.md
  - skills/dotnet-modern-development/SKILL.md
  - skills/coding-standards/SKILL.md
  - skills/testing/SKILL.md
  - skills/documentation/SKILL.md
---

# Blazor Expert Agent

Specialized guidance for designing, building, and optimizing Blazor components for the CymruBlazor component library implementing the NHS Wales Design System.

## Responsibilities

### Component Design & Architecture

- Design single-responsibility components with clear input/output contracts
- Recommend composition patterns (parent-child, sibling communication)
- Guide component lifecycle management (OnInitialized, OnParametersSet, Dispose)
- Optimize rendering and change detection
- Balance reusability vs. specificity

### State Management

- Design component-local state for UI concerns
- Recommend service-based state for cross-component data
- Guide cascade parameters for parent-to-child data flow
- Recommend patterns for complex state (Redux-like cascading, custom services)

### Async Operations & Interop

- Guide async component initialization
- Recommend cancellation token usage
- Review JavaScript interop for correctness and performance
- Advise on browser API usage (localStorage, fetch, etc.)

### Testing & Quality

- Recommend unit test strategies for components (behavior tests)
- Advise on integration testing with bUnit
- Guide testing of event handlers, forms, and validation
- Recommend performance testing for render-heavy components

### Performance Optimization

- Identify unnecessary re-renders
- Recommend `@key` directive usage
- Guide virtual scrolling for large lists
- Advise on lazy-loading and code splitting
- Optimize interop calls to minimize marshaling

## Component Design Decision Tree

### Is This a Presentation Component?

**Yes** → Stateless, receives data via parameters, emits events via callbacks

```csharp
// ✅ PRESENTATION COMPONENT: Pure presentation, no side effects
@typeparam TItem

<article class="@BuildCssClass("cymru-card", Class)">
    @if (!string.IsNullOrWhiteSpace(ImageSrc))
    {
        <div class="cymru-card__img-wrapper">
            <img src="@ImageSrc" alt="@ImageAlt" />
        </div>
    }
    <div class="cymru-card__content">
        <h2 class="cymru-card__heading">@Heading</h2>
        <p class="cymru-card__description">@Description</p>
        @if (OnClick.HasDelegate)
        {
            <button class="cymru-button cymru-button--primary" @onclick="OnClick">
                Learn more
            </button>
        }
    </div>
</article>

@code {
    [Parameter, EditorRequired]
    public string Heading { get; set; } = default!;

    [Parameter, EditorRequired]
    public string Description { get; set; } = default!;

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public string? ImageSrc { get; set; }

    [Parameter]
    public string? Class { get; set; }
}
```

**No** → Container component that loads data, manages state, handles side effects

### Does This Component Need Data from Multiple Layers?

**Yes** → Use cascading parameters for cross-cutting concerns (auth, theme), inject services for data

```csharp
// ✅ CASCADING PARAMETERS for theme and global state
<CascadingValue Value="currentTheme" Name="CymruTheme" IsFixed="true">
    <CascadingValue Value="currentUser" Name="CurrentUser" IsFixed="true">
        <Button Variant="ButtonVariant.Primary">Apply Theme</Button>
    </CascadingValue>
</CascadingValue>

// ✅ SERVICE INJECTION for utilities (logging, analytics)
@inject ILogger<ButtonComponent> Logger
@inject IAnalyticsService Analytics

@code {
    [CascadingParameter(Name = "CymruTheme")]
    public CymruTheme CurrentTheme { get; set; } = CymruTheme.Default;

    [CascadingParameter(Name = "CurrentUser")]
    public User? CurrentUser { get; set; }
}
```

**No** → Pass data via parameters; keep dependencies minimal

### Is This Component Large (>200 lines)?

**Yes** → Decompose into smaller components

```csharp
// ❌ TOO LARGE: Button component trying to do everything
<button class="@CssClass" @onclick="HandleClick" disabled="@IsDisabled">
    <!-- 100 lines of rendering logic for different variants -->
    <!-- 50 lines of form handling -->
    <!-- 40 lines of accessibility logic -->
    <!-- 30 lines of analytics tracking -->
    @ChildContent
</button>

// ✅ COMPOSED: Separate concerns
<Button Variant="ButtonVariant.Primary" OnClick="HandlePrimaryAction">
    Save
</Button>
<Button Variant="ButtonVariant.Secondary" OnClick="HandleSecondaryAction">
    Cancel
</Button>
<IconButton Icon="icon-edit" Tooltip="Edit" OnClick="HandleEdit" />
```

### Does This Component Re-render Excessively?

**Yes** → Use `@key`, `ShouldRender()`, or `ComponentBase.SetParametersAsync()`

```csharp
// ❌ RE-RENDERS ON EVERY PARENT CHANGE
@foreach (var item in Buttons)
{
    <Button Variant="item.Variant">@item.Label</Button>
}

// ✅ KEYED: Only re-renders when item changes
@foreach (var item in Buttons)
{
    <Button Variant="item.Variant" @key="item.Id">@item.Label</Button>
}

// ✅ MANUAL RENDER CONTROL
@code {
    protected override bool ShouldRender()
    {
        // Only re-render if variant changed
        return Variant != previousVariant || ChildContent != previousContent;
    }
}
```

**No** → Continue with standard rendering

## State Management Patterns

### Local Component State (UI Concerns Only)

```csharp
// ✅ LOCAL STATE: Dropdown open/close, form input values, loading indicators
@page "/patient-form"

<form @onsubmit="HandleSubmit">
    <input @bind="firstName" placeholder="First Name" />
    <input @bind="lastName" placeholder="Last Name" />
    <button type="submit" disabled="@isSubmitting">
        @(isSubmitting ? "Submitting..." : "Create")
    </button>
</form>

@code {
    private string firstName = "";
    private string lastName = "";
    private bool isSubmitting = false;

    private async Task HandleSubmit()
    {
        isSubmitting = true;
        try
        {
            await PatientService.CreateAsync(firstName, lastName);
            // Success
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### Service-Based State (Shared Across Components)

```csharp
// ✅ INJECTED SERVICE: Authentication, current user, application settings
@inject IAuthenticationService AuthService
@inject ICurrentUserService CurrentUser

@page "/patient-dashboard"

@if (CurrentUser.IsAuthenticated)
{
    <p>Welcome, @CurrentUser.User.FirstName</p>
}

@code {
    private User? user;

    protected override async Task OnInitializedAsync()
    {
        user = await CurrentUser.GetCurrentUserAsync();
    }
}
```

### Cascading Parameters (Parent-to-Child, Read-Only)

```csharp
// ✅ PARENT COMPONENT
<CascadingValue Value="currentPatient">
    <PatientDetail />
</CascadingValue>

// ✅ CHILD COMPONENT
@code {
    [CascadingParameter]
    public Patient CurrentPatient { get; set; } = default!;
}
```

### Complex State: Custom Service with Observable Pattern

```csharp
// ✅ STATE SERVICE: For complex state shared across many components
public interface IPatientStateService
{
    Patient? CurrentPatient { get; }
    IAsyncEnumerable<Patient> PatientChanged { get; }
    Task SelectPatientAsync(Guid id);
}

public sealed class PatientStateService : IPatientStateService, IAsyncDisposable
{
    private readonly Channel<Patient?> _patientChannel = Channel.CreateUnbounded<Patient?>();
    private Patient? _currentPatient;

    public Patient? CurrentPatient => _currentPatient;
    public IAsyncEnumerable<Patient> PatientChanged => _patientChannel.Reader.ReadAllAsync();

    public async Task SelectPatientAsync(Guid id)
    {
        _currentPatient = await _repository.GetByIdAsync(id);
        await _patientChannel.Writer.WriteAsync(_currentPatient);
    }

    public async ValueTask DisposeAsync()
    {
        _patientChannel.Writer.Complete();
    }
}

// ✅ USAGE IN COMPONENT
@inject IPatientStateService PatientState

@page "/patient-details"

@if (currentPatient is not null)
{
    <h1>@currentPatient.FirstName @currentPatient.LastName</h1>
}

@code {
    private Patient? currentPatient;

    protected override async Task OnInitializedAsync()
    {
        currentPatient = PatientState.CurrentPatient;

        await foreach (var patient in PatientState.PatientChanged)
        {
            currentPatient = patient;
            StateHasChanged();
        }
    }
}
```

## Async Operations & Error Handling

### Component Initialization

```csharp
// ✅ LOAD DATA ON INIT, HANDLE ERRORS
@page "/patient/{Id}"
@inject IPatientService service
@inject ILogger<PatientDetail> logger

@if (patient is null)
{
    <p>Loading...</p>
}
else if (error is not null)
{
    <div class="alert alert-danger">
        Error: @error
        <button @onclick="Retry">Retry</button>
    </div>
}
else
{
    <h1>@patient.FirstName @patient.LastName</h1>
}

@code {
    [Parameter]
    public Guid Id { get; set; }

    private Patient? patient;
    private string? error;
    private CancellationTokenSource? cts;

    protected override async Task OnInitializedAsync()
    {
        cts = new CancellationTokenSource();
        await LoadPatientAsync();
    }

    private async Task LoadPatientAsync()
    {
        try
        {
            error = null;
            patient = await service.GetPatientAsync(Id, cts!.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Patient load cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load patient {PatientId}", Id);
            error = "Failed to load patient. Please try again.";
        }
    }

    private async Task Retry()
    {
        await LoadPatientAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
```

### Event Handling

```csharp
// ✅ ASYNC EVENT HANDLERS WITH ERROR HANDLING
@page "/patient-form"

<form @onsubmit="HandleSubmitAsync">
    <input @bind="firstName" />
    <input @bind="lastName" />
    <button type="submit" disabled="@isSubmitting">
        @(isSubmitting ? "Creating..." : "Create")
    </button>
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <p class="error">@errorMessage</p>
    }
</form>

@code {
    private string firstName = "";
    private string lastName = "";
    private bool isSubmitting = false;
    private string errorMessage = "";

    private async Task HandleSubmitAsync()
    {
        isSubmitting = true;
        errorMessage = "";

        try
        {
            await PatientService.CreateAsync(firstName, lastName);
            firstName = "";
            lastName = "";
            // Success—navigate or show confirmation
        }
        catch (ValidationException ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

## JavaScript Interop Best Practices

### Minimal Interop Surface

```csharp
// ✅ ENCAPSULATED JS INTEROP
public sealed class BrowserStorageService
{
    private readonly IJSRuntime _js;

    public BrowserStorageService(IJSRuntime js) => _js = js;

    public async ValueTask SetItemAsync(string key, string value)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException ex)
        {
            // Log and handle
        }
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException ex)
        {
            return null;
        }
    }
}

// ❌ AVOID: Direct JS interop scattered across components
// @inject IJSRuntime js
// await js.InvokeVoidAsync("eval", "someJsCode()");
```

### JS Module Interop (Preferred Over Global Functions)

```csharp
// ✅ MODULE: wwwroot/js/patient-utils.js
export async function validatePatientEmail(email) {
    // Validation logic
    return isValid;
}

// ✅ COMPONENT
@page "/patient-form"
@implements IAsyncDisposable

<input @bind="email" @onchange="ValidateEmailAsync" />

@code {
    private string email = "";
    private IJSObjectReference? module;

    protected override async Task OnInitializedAsync()
    {
        module = await JS.InvokeAsync<IJSObjectReference>(
            "import", "./js/patient-utils.js");
    }

    private async Task ValidateEmailAsync(ChangeEventArgs e)
    {
        var isValid = await module!.InvokeAsync<bool>("validatePatientEmail", e.Value);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (module is not null)
            await module.DisposeAsync();
    }
}
```

## Component Testing with bUnit

```csharp
// ✅ TEST COMPONENT BEHAVIOR
public sealed class PatientCardTests : TestContext
{
    [Fact]
    public async Task Render_WithPatient_DisplaysPatientInfo()
    {
        // Arrange
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
        var component = RenderComponent<PatientCard>(parameters => parameters
            .Add(p => p.Patient, patient));

        // Act & Assert
        component.Find("h3").TextContent.Should().Contain("John");
        component.Find("h3").TextContent.Should().Contain("Doe");
    }

    [Fact]
    public async Task OnSelect_WhenClicked_InvokesCallback()
    {
        // Arrange
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "John" };
        var callbackInvoked = false;
        var invokedPatient = (Patient?)null;
        var component = RenderComponent<PatientCard>(parameters => parameters
            .Add(p => p.Patient, patient)
            .Add(p => p.OnSelect, EventCallback.Factory.Create<Patient>(this, p =>
            {
                callbackInvoked = true;
                invokedPatient = p;
            })));

        // Act
        component.Find("button").Click();

        // Assert
        callbackInvoked.Should().BeTrue();
        invokedPatient.Should().Be(patient);
    }
}
```

## Performance Optimization Checklist

- [ ] Components under 200 lines; decompose if larger
- [ ] `@key` directive used for lists to prevent unnecessary re-renders
- [ ] Event handlers are `async Task` or `async Task<T>`, not `void`
- [ ] `CancellationToken` passed to all async operations
- [ ] `ShouldRender()` overridden only when necessary for performance
- [ ] Large lists use virtual scrolling or pagination
- [ ] JS interop calls minimized and batched where possible
- [ ] Components dispose subscriptions and JS modules
- [ ] Cascading parameters used for cross-cutting concerns (auth, theme)
- [ ] Child components accept `@key` for list rendering
- [ ] Error handling present for async operations
- [ ] Loading and error states clearly indicated to users

## Anti-Patterns to Avoid

```csharp
// ❌ COMPONENT TOO LARGE: Mix of concerns
@page "/patient-management"

<!-- 100 lines of form -->
<!-- 80 lines of list -->
<!-- 60 lines of search -->
<!-- 40 lines of dialogs -->

// ✅ DECOMPOSED
@page "/patient-management"

<PatientSearchBar OnSearch="HandleSearch" />
<PatientList Patients="patients" />
<PatientCreateDialog @ref="createDialog" />

// ❌ EVENT HANDLER WITHOUT ERROR HANDLING
private async Task HandleClick()
{
    await PatientService.CreateAsync(model);  // Unhandled exception crashes app
}

// ✅ WITH ERROR HANDLING
private async Task HandleClick()
{
    try
    {
        await PatientService.CreateAsync(model);
    }
    catch (Exception ex)
    {
        errorMessage = ex.Message;
    }
}

// ❌ NO KEY IN LOOP: Re-renders entire list
@foreach (var patient in patients)
{
    <PatientCard Patient="patient" />
}

// ✅ WITH KEY: Only re-renders changed items
@foreach (var patient in patients)
{
    <PatientCard Patient="patient" @key="patient.Id" />
}
```

## Related Agents & Skills

- `.github/agents/architect.md` – Architectural patterns for component composition
- `.github/agents/csharp-expert.md` – C# language features used in components
- `skills/testing/SKILL.md` – Testing philosophy and patterns
- `skills/coding-standards/SKILL.md` – Code quality standards
