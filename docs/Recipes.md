# Recipes

Complete, working pages that show how the components fit together. Each one is also a page in the Demo
(Getting Started, Recipes), where it is scanned for accessibility problems in every theme. The code below is the
copy-and-adapt version.

## List page

A page header with an action above a sortable, paged table.

```razor
<CyPageHeader Title="Appointments" Eyebrow="Cardiff clinic" Subtitle="Today's appointments, sortable and paged.">
    <Actions>
        <CyButton>New appointment</CyButton>
    </Actions>
</CyPageHeader>

<CyPage>
    <CyDataTable TItem="Appointment" Caption="Today's appointments" CaptionVisuallyHidden="true"
                 Columns="_columns" Items="_rows" PageSize="10" KeySelector="a => a.Id" />
</CyPage>

@code {
    private IReadOnlyList<CyDataColumn<Appointment>> _columns = [
        new() { Header = "Patient", Value = a => a.Patient, Sortable = true, RowHeader = true },
        new() { Header = "Clinic", Value = a => a.Clinic, Sortable = true },
        new() { Header = "Minutes", Value = a => a.Minutes, Sortable = true, Align = CyColumnAlign.End }
    ];
}
```

## Editor page

A form with a toolbar and a guard against losing unsaved changes. Try editing a field, then following a link.

```razor
<CyUnsavedChanges Dirty="IsDirty" SaveState="_state" SavedAt="_savedAt" />

<CyPageHeader Title="Edit referral" Eyebrow="Genomics" />

<CyToolbar Label="Referral actions">
    <CyButton Disabled="@(!CanSave)" OnClick="SaveAsync">Save</CyButton>
    <CyButton Variant="ComponentColour.Secondary" Disabled="@(!IsDirty)" OnClick="Discard">Discard changes</CyButton>
</CyToolbar>

<CyPage Width="CyPageWidth.Narrow">
    <CyPageSection Heading="Patient">
        <CyStack>
            <CyField Label="Full name" Required="true" Context="field"
                     Error="@(string.IsNullOrWhiteSpace(_name) ? "Enter the patient's full name" : null)">
                <input @attributes="field.Attributes" class="cy-input" type="text" @bind="_name" @bind:event="oninput" />
            </CyField>
        </CyStack>
    </CyPageSection>
</CyPage>

@code {
    private string _name = "Gwen Davies", _savedName = "Gwen Davies";
    private CySaveState _state;
    private DateTimeOffset? _savedAt;

    private bool IsDirty => _name != _savedName;
    private bool CanSave => IsDirty && !string.IsNullOrWhiteSpace(_name) && _state != CySaveState.Saving;

    private async Task SaveAsync()
    {
        _state = CySaveState.Saving;
        await Task.Delay(800);              // your save call
        _savedName = _name;
        _savedAt = DateTimeOffset.Now;
        _state = CySaveState.Saved;
    }

    private void Discard() => _name = _savedName;
}
```

## Wizard

A multi-step task that ends with a check-your-answers summary whose Change actions return to the owning step.

```razor
<CyStepper Label="Request a test" Steps="_steps" @bind-Current="_current" Clickable="true" />

@if (_current == 0) { <CyTextBox @bind-Value="_name" Label="Patient name" /> }
else if (_current == 1) { <CyTextBox @bind-Value="_test" Label="Test requested" /> }
else
{
    <CySummaryList>
        <CySummaryRow Key="Patient" Value="@_name" OnChange="@(() => _current = 0)" />
        <CySummaryRow Key="Test" Value="@_test" OnChange="@(() => _current = 1)" />
    </CySummaryList>
}

<CyButton Variant="ComponentColour.Secondary" Disabled="@(_current == 0)" OnClick="Back">Back</CyButton>
<CyButton OnClick="Next">Next</CyButton>

@code {
    private int _current;
    private string? _name, _test;

    private readonly IReadOnlyList<CyStepItem> _steps =
    [
        new CyStepItem("Patient"),
        new CyStepItem("Test"),
        new CyStepItem("Check your answers")
    ];

    private void Back() => _current = Math.Max(0, _current - 1);
    private void Next() => _current = Math.Min(_steps.Count - 1, _current + 1);
}
```

## Tool workspace

Palette, canvas and inspector side by side. Selection is a pressed button, so it is announced and not only coloured.

```razor
<CyWorkspace Style="--cy-workspace-height: 70vh">
    <CyWorkspacePane Title="Palette" Collapsible="true" Width="12rem" MinWidth="8rem">...</CyWorkspacePane>
    <CyWorkspacePane Title="Canvas" Fill="true">...</CyWorkspacePane>
    <CyWorkspacePane Title="Inspector" Collapsible="true" Width="14rem" MinWidth="10rem">...</CyWorkspacePane>
</CyWorkspace>

@* Selecting is a button with aria-pressed, so the selection is announced, not only coloured. *@
<CyButton Variant="ComponentColour.Tertiary" aria-pressed="@(index == _selected ? "true" : "false")"
          OnClick="@(() => _selected = index)">@_questions[index]</CyButton>

@code {
    private readonly List<string> _questions = [];
    private int? _selected;

    private void Add(string kind)
    {
        _questions.Add(kind);
        _selected = _questions.Count - 1;
    }
}
```

## Layout cookbook

Use `CyCluster` for things on a line that may wrap, `CyStack` for a column, `CyGrid` for equal columns, `CyPage` for the page width policy and `CyContainer` to cap a block inside a region you already have.

```razor
@* Action row: items on one line, wrapping when there is no room *@
<CyCluster JustifyContent="JustifyContent.SpaceBetween">
    <strong>Referrals</strong>
    <CyCluster Gap="ComponentSize.Small"> ...buttons... </CyCluster>
</CyCluster>

@* Form row: two equal columns *@
<CyGrid Columns="GridColumns.Two"> ...two fields... </CyGrid>

@* Card grid: as many columns as fit *@
<CyGrid Columns="GridColumns.Auto"> ...cards... </CyGrid>

@* Page width: one policy per page *@
<CyPage Width="CyPageWidth.Narrow"> ... </CyPage>

@* Vertical rhythm for a single column of content *@
<CyStack Gap="ComponentSize.Medium"> ... </CyStack>

@* CyContainer caps width inside a region you already have; CyPage is the page-level policy. *@
```

## Localisation example

The library takes every string as a parameter, so your app owns one scoped service and hands strings to components. Keep your own strings in a separate record beside the library strings, and mark other-language text with `lang`. There is no `ILocalisationService` type in the library; the service is your own `AppStrings` (see the Localisation page in the Demo).

```csharp
// 1. Your strings: a plain record, one instance per language.
public sealed record ClinicStrings(string Heading, string Intro, string Count)
{
    public static ClinicStrings English { get; } = new("Clinic referrals", "Check each referral before you book.", "{0} referrals waiting");
    public static ClinicStrings Welsh   { get; } = new("Atgyfeiriadau clinig", "Gwiriwch bob atgyfeiriad cyn archebu.", "{0} atgyfeiriad yn aros");
}

// 2. Resolve it beside the library strings, from the same service and language.
public sealed class AppStrings
{
    public AppLanguage Language { get; private set; }
    public event Action? Changed;

    public LibraryStrings Library => Language == AppLanguage.Welsh ? WelshLibrary : EnglishLibrary;
    public ClinicStrings Clinic   => Language == AppLanguage.Welsh ? ClinicStrings.Welsh : ClinicStrings.English;
    public string LangTag => Language == AppLanguage.Welsh ? "cy" : "en";

    public void SetLanguage(AppLanguage language) { Language = language; Changed?.Invoke(); }
}

// 3. Use both in a page, and mark Welsh text with lang so assistive technology pronounces it correctly.
@inject AppStrings T
<CyBreadcrumb AriaLabel="@T.Library.BreadcrumbLabel"> ... </CyBreadcrumb>
<h1 lang="@T.LangTag">@T.Clinic.Heading</h1>
```

## Razor trap: text beside `@`

Razor reads `v@q` as an email address, so it does not insert the value of `q`. Use `@(...)` whenever static text sits
right beside the expression.

```razor
@* Wrong: v@q is treated as an email address and printed as-is *@
<p>Version v@version</p>

@* Right: explicit expression *@
<p>Version v@(version)</p>
<p>@(count)x @(unit)</p>
```
