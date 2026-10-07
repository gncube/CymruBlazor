using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using CymruBlazor.Components.Core;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// Chooses files (button, keyboard or drag and drop onto the field), checks them against the limits you set, lists them
/// with progress and per-file errors, and hands each to <em>your</em> upload callback. Built on the framework's
/// <see cref="InputFile"/>; the visible drop zone is the file input itself, so picking and dropping are native and need
/// no script.
/// </summary>
/// <remarks>
/// <para>
/// <b>Security: client-side checks are not enforcement.</b> The name, size and content type of every file come from the
/// browser and can be forged by anyone who is not using your page. The checks here (<see cref="Accept"/>,
/// <see cref="MaxFileSize"/>, <see cref="MaxFiles"/>, <see cref="MaxTotalSize"/>) exist so honest users get a clear
/// message before uploading; they protect nothing. Whatever receives the bytes must re-validate size, extension
/// <em>and content</em> (for example the file's magic bytes), scan for malware, choose its own storage name (never use
/// the supplied one as a path), store outside the web root, and authorise the request.
/// </para>
/// <para>
/// <b>No endpoint.</b> The library never sends a file anywhere. Set <see cref="Upload"/> to receive each file as a
/// <see cref="CyFileUploadContext"/> and send it however you like (a typed <c>HttpClient</c>, a service call). Without
/// <see cref="Upload"/> the component only validates and lists; read the files from <see cref="Files"/> or
/// <see cref="OnFilesChanged"/> when your form is submitted. Always read through
/// <see cref="CyFileUploadContext.OpenReadStream"/> (or <c>IBrowserFile.OpenReadStream</c> with an explicit size limit):
/// the framework's default limit is 500 KB. In Blazor Server the bytes travel over the SignalR connection, so large
/// files are better uploaded straight from the browser to an API.
/// </para>
/// <para>
/// <b>Accessibility.</b> The label, hint, limits, rejection messages and any <see cref="Error"/> are linked with
/// <c>aria-describedby</c>. Rejections and failures appear in <c>role="alert"</c> regions; "added", "uploading",
/// "uploaded" and "removed" are announced politely. Percentage progress is shown on a progress bar but not announced,
/// so a screen reader is not flooded. Every button names its file.
/// </para>
/// </remarks>
public partial class CyFileUpload : CyComponentBase, IDisposable
{
    private const long DefaultMaxFileSize = 5L * 1024 * 1024;
    private const int SelectionHardLimit = 1000;

    private static readonly CyFileUploadText DefaultText = new();

    private readonly List<CyFileItem> _items = [];
    private readonly CancellationTokenSource _disposed = new();
    private List<CyFileRejection> _rejections = [];
    private IReadOnlyList<string> _acceptTokens = [];
    private InputFile? _inputFile;
    private int _inputKey;
    private int _nextId;
    private bool _focusInput;
    private bool _isDisposed;
    private string _status = string.Empty;

    /// <summary>The visible name of the field. Required: every field needs a programmatic label.</summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    /// <summary>Optional supporting text under the label.</summary>
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>Marks the field as required (visual marker and <c>aria-required</c>); enforce it with your own validation.</summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>Disables choosing, removing and uploading.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>An error to show for the field as a whole, set by you (for example "Choose a file"). Linked to the input.</summary>
    [Parameter]
    public string? Error { get; set; }

    /// <summary>
    /// The file types to offer: a comma-separated list of extensions (<c>.pdf</c>), MIME types (<c>image/png</c>) or
    /// wildcards (<c>image/*</c>). Written to the input's <c>accept</c> attribute (a picker hint only) and re-checked in
    /// C# against the file's extension and reported content type. Not enforcement. A value that is not in this format
    /// throws. When only MIME types are listed, a file with no reported content type is not accepted.
    /// </summary>
    [Parameter]
    public string? Accept { get; set; }

    /// <summary>Allows more than one file. Default false.</summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>The largest file in bytes. Default 5 MiB. Also the limit passed to <c>OpenReadStream</c>. Must be greater than 0.</summary>
    [Parameter]
    public long MaxFileSize { get; set; } = DefaultMaxFileSize;

    /// <summary>The most files at once. Defaults to 10 when <see cref="Multiple"/>, otherwise 1.</summary>
    [Parameter]
    public int? MaxFiles { get; set; }

    /// <summary>Optional limit on the combined size of all files, in bytes.</summary>
    [Parameter]
    public long? MaxTotalSize { get; set; }

    /// <summary>
    /// Called once per file to send it somewhere. Return <see cref="CyUploadResult.Success"/> or
    /// <see cref="CyUploadResult.Failure(string?)"/>. An exception is shown as the generic failure text; its message is
    /// never displayed. Honour <see cref="CyFileUploadContext.CancellationToken"/>.
    /// </summary>
    [Parameter]
    public Func<CyFileUploadContext, Task<CyUploadResult>>? Upload { get; set; }

    /// <summary>Start uploading as soon as files are added (default true). When false a button starts them. Needs <see cref="Upload"/>.</summary>
    [Parameter]
    public bool AutoUpload { get; set; } = true;

    /// <summary>How many files upload at the same time. Default 1 (one after another).</summary>
    [Parameter]
    public int MaxParallel { get; set; } = 1;

    /// <summary>Raised with the current list whenever files are added or removed. Not raised for rejected files.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<CyFileItem>> OnFilesChanged { get; set; }

    /// <summary>Raised with the files that were not added and why.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<CyFileRejection>> OnRejected { get; set; }

    /// <summary>The phrases the component shows and announces; defaults to English.</summary>
    [Parameter]
    public CyFileUploadText? Text { get; set; }

    /// <summary>A snapshot of the files currently listed.</summary>
    public IReadOnlyList<CyFileItem> Files => _items.ToList();

    /// <inheritdoc />
    protected override string BaseCssClass => "cy-file-upload";

    private CyFileUploadText EffectiveText => Text ?? DefaultText;

    private int EffectiveMaxFiles => Multiple ? (MaxFiles ?? 10) : 1;

    private string InputId => $"{Id}-input";

    private string HintId => $"{Id}-hint";

    private string LimitsId => $"{Id}-limits";

    private string RejectionsId => $"{Id}-rejections";

    private string ErrorId => $"{Id}-error";

    private string FileListLabel => Format(EffectiveText.FileList, Label);

    /// <inheritdoc />
    protected override string BuildCssClass() =>
        CssBuilder.Empty
            .AddClass(BaseCssClass)
            .AddClass("cy-field")
            .AddClass("cy-file-upload--disabled", Disabled)
            .AddClass("cy-field--invalid", !string.IsNullOrWhiteSpace(Error))
            .AddClass(Class)
            .Build();

    /// <inheritdoc />
    protected override void ValidateParameters()
    {
        base.ValidateParameters();

        if (string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(CyFileUpload)}.{nameof(Label)} must not be empty: every field needs a programmatic label.");
        }

        if (MaxFileSize <= 0)
        {
            throw new InvalidOperationException($"{nameof(CyFileUpload)}.{nameof(MaxFileSize)} must be greater than 0.");
        }

        if (MaxFiles is < 1)
        {
            throw new InvalidOperationException($"{nameof(CyFileUpload)}.{nameof(MaxFiles)} must be at least 1.");
        }

        if (MaxTotalSize is <= 0)
        {
            throw new InvalidOperationException($"{nameof(CyFileUpload)}.{nameof(MaxTotalSize)} must be greater than 0.");
        }

        if (MaxParallel < 1)
        {
            throw new InvalidOperationException($"{nameof(CyFileUpload)}.{nameof(MaxParallel)} must be at least 1.");
        }

        if (!CyFileAccept.TryParse(Accept, out var tokens, out var invalid))
        {
            throw new InvalidOperationException(
                $"{nameof(CyFileUpload)}.{nameof(Accept)} must be a comma-separated list of extensions (.pdf), MIME types (image/png) or wildcards (image/*). Received '{invalid}'.");
        }

        _acceptTokens = tokens;
    }

    private string? DescribedBy
    {
        get
        {
            var ids = new List<string>();

            if (!string.IsNullOrWhiteSpace(HintText))
            {
                ids.Add(HintId);
            }

            ids.Add(LimitsId);

            if (_rejections.Count > 0)
            {
                ids.Add(RejectionsId);
            }

            if (!string.IsNullOrWhiteSpace(Error))
            {
                ids.Add(ErrorId);
            }

            return string.Join(' ', ids);
        }
    }

    private Dictionary<string, object> InputAttributes
    {
        get
        {
            var attributes = new Dictionary<string, object>();

            if (AdditionalAttributes is not null)
            {
                foreach (var pair in AdditionalAttributes)
                {
                    attributes[pair.Key] = pair.Value;
                }
            }

            if (_acceptTokens.Count > 0)
            {
                attributes["accept"] = string.Join(',', _acceptTokens);
            }

            if (Multiple)
            {
                attributes["multiple"] = true;
            }

            if (Disabled)
            {
                attributes["disabled"] = true;
            }

            attributes["id"] = InputId;
            attributes["class"] = "cy-file-upload__input";
            attributes["aria-required"] = Required ? "true" : "false";
            attributes["aria-invalid"] = string.IsNullOrWhiteSpace(Error) ? "false" : "true";
            attributes["aria-describedby"] = DescribedBy!;

            return attributes;
        }
    }

    private string LimitsText
    {
        get
        {
            var text = EffectiveText;
            var parts = new List<string> { Format(text.LimitSize, SizeText(MaxFileSize)) };

            if (_acceptTokens.Count > 0)
            {
                parts.Add(Format(text.LimitTypes, string.Join(", ", _acceptTokens)));
            }

            if (EffectiveMaxFiles > 1)
            {
                parts.Add(Format(text.LimitCount, EffectiveMaxFiles));
            }

            if (MaxTotalSize is { } total)
            {
                parts.Add(Format(text.LimitTotal, SizeText(total)));
            }

            return string.Join(' ', parts);
        }
    }

    // ---- selecting

    private async Task OnSelectedAsync(InputFileChangeEventArgs args)
    {
        if (Disabled)
        {
            return;
        }

        var text = EffectiveText;
        var rejections = new List<CyFileRejection>();
        var accepted = new List<CyFileItem>();

        if (args.FileCount > SelectionHardLimit)
        {
            rejections.Add(new CyFileRejection(
                string.Empty,
                CyFileRejectionReason.TooMany,
                Format(text.SelectionTooLarge, EffectiveMaxFiles)));
        }
        else
        {
            IReadOnlyList<CyFileItem> kept = Multiple ? _items : Array.Empty<CyFileItem>();
            var existingCount = kept.Count;
            var totalSize = kept.Sum(item => item.Size);

            foreach (var file in args.GetMultipleFiles(Math.Max(1, args.FileCount)))
            {
                var rejection = Check(file, kept, accepted, existingCount, totalSize);

                if (rejection is not null)
                {
                    rejections.Add(rejection);
                    continue;
                }

                accepted.Add(new CyFileItem($"{Id}-f{++_nextId}", file));
                totalSize += file.Size;
            }
        }

        _rejections = rejections;
        _status = string.Empty;

        if (accepted.Count > 0)
        {
            if (!Multiple)
            {
                foreach (var existing in _items)
                {
                    existing.Cancellation?.Cancel();
                }

                _items.Clear();
            }

            var queue = Upload is not null && AutoUpload;

            foreach (var item in accepted)
            {
                item.Queued = queue;
            }

            _items.AddRange(accepted);
            _status = accepted.Count == 1
                ? Format(text.AddedOne, accepted[0].Name)
                : Format(text.AddedMany, accepted.Count);

            await NotifyFilesChangedAsync();
        }

        if (rejections.Count > 0 && OnRejected.HasDelegate)
        {
            await OnRejected.InvokeAsync(rejections);
        }

        if (accepted.Count > 0)
        {
            await PumpAsync();
        }
    }

    private CyFileRejection? Check(
        IBrowserFile file,
        IReadOnlyList<CyFileItem> kept,
        List<CyFileItem> accepted,
        int existingCount,
        long totalSize)
    {
        var text = EffectiveText;
        var name = file.Name;

        if (file.Size == 0)
        {
            return new CyFileRejection(name, CyFileRejectionReason.Empty, Format(text.Empty, name));
        }

        if (file.Size > MaxFileSize)
        {
            return new CyFileRejection(
                name,
                CyFileRejectionReason.TooLarge,
                Format(text.TooLarge, name, SizeText(file.Size), SizeText(MaxFileSize)));
        }

        if (!CyFileAccept.Matches(_acceptTokens, name, file.ContentType))
        {
            return new CyFileRejection(
                name,
                CyFileRejectionReason.WrongType,
                Format(text.WrongType, name, string.Join(", ", _acceptTokens)));
        }

        if (kept.Concat(accepted).Any(item => item.Size == file.Size && string.Equals(item.Name, name, StringComparison.Ordinal)))
        {
            return new CyFileRejection(name, CyFileRejectionReason.Duplicate, Format(text.Duplicate, name));
        }

        if (existingCount + accepted.Count >= EffectiveMaxFiles)
        {
            return new CyFileRejection(
                name,
                CyFileRejectionReason.TooMany,
                Format(text.TooMany, name, EffectiveMaxFiles));
        }

        if (MaxTotalSize is { } cap && totalSize + file.Size > cap)
        {
            return new CyFileRejection(
                name,
                CyFileRejectionReason.TooMuch,
                Format(text.TooMuch, name, SizeText(cap)));
        }

        return null;
    }

    // ---- uploading

    private async Task StartAsync()
    {
        foreach (var item in _items.Where(i => i.Status == CyFileStatus.Ready))
        {
            item.Queued = true;
        }

        await PumpAsync();
    }

    private async Task PumpAsync()
    {
        while (!_isDisposed && Upload is not null)
        {
            if (_items.Count(i => i.Status == CyFileStatus.Uploading) >= MaxParallel)
            {
                return;
            }

            var next = _items.FirstOrDefault(i => i.Status == CyFileStatus.Ready && i.Queued);

            if (next is null)
            {
                return;
            }

            // UploadOneAsync marks the item as uploading before its first await, so this loop always makes progress.
            _ = UploadOneAsync(next);
        }

        await Task.CompletedTask;
    }

    private async Task UploadOneAsync(CyFileItem item)
    {
        var text = EffectiveText;
        item.Status = CyFileStatus.Uploading;
        item.Progress = 0;
        item.Error = null;
        item.Queued = false;

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_disposed.Token);
        item.Cancellation = cancellation;
        _status = Format(text.Uploading, item.Name);
        Refresh();

        try
        {
            var progress = new ImmediateProgress(bytes =>
            {
                item.Progress = Math.Min(bytes, item.Size);
                Refresh();
            });

            var context = new CyFileUploadContext(item.File, MaxFileSize, progress, cancellation.Token);
            var result = await Upload!(context);

            cancellation.Token.ThrowIfCancellationRequested();

            if (result is { Succeeded: true })
            {
                item.Status = CyFileStatus.Uploaded;
                item.Progress = item.Size;
                _status = Format(text.Uploaded, item.Name);
            }
            else
            {
                var message = result?.Message;
                Fail(item, string.IsNullOrWhiteSpace(message) ? Format(text.UploadFailed, item.Name) : message);
            }
        }
        catch (OperationCanceledException)
        {
            if (!_isDisposed)
            {
                Fail(item, Format(text.UploadCancelled, item.Name));
            }
        }
        catch (IOException ex) when (ex.Message.Contains("exceeds", StringComparison.OrdinalIgnoreCase))
        {
            // The framework refused to read past MaxFileSize: the file was larger than the browser reported.
            Fail(item, Format(text.TooLargeActual, item.Name, SizeText(MaxFileSize)));
        }
#pragma warning disable CA1031 // A consumer's upload callback may throw anything; show the generic failure, never the exception text.
        catch (Exception)
#pragma warning restore CA1031
        {
            Fail(item, Format(text.UploadFailed, item.Name));
        }
        finally
        {
            item.Cancellation = null;
        }

        if (_isDisposed)
        {
            return;
        }

        Refresh();
        await PumpAsync();
    }

    private static void Fail(CyFileItem item, string message)
    {
        item.Status = CyFileStatus.Failed;
        item.Error = message;
        item.Queued = false;
    }

    private async Task RetryAsync(CyFileItem item)
    {
        item.Status = CyFileStatus.Ready;
        item.Error = null;
        item.Queued = true;
        await PumpAsync();
    }

    private static Task CancelAsync(CyFileItem item)
    {
        item.Cancellation?.Cancel();
        return Task.CompletedTask;
    }

    private async Task RemoveAsync(CyFileItem item)
    {
        item.Cancellation?.Cancel();
        _items.Remove(item);
        _rejections = [];
        _status = Format(EffectiveText.Removed, item.Name);

        // A native file input keeps its last selection, so choosing the same file again would raise no change event.
        // A new element (and focus moved to it, so a keyboard user keeps their place) avoids that.
        _inputKey++;
        _focusInput = true;

        await NotifyFilesChangedAsync();
    }

    private Task NotifyFilesChangedAsync() =>
        OnFilesChanged.HasDelegate ? OnFilesChanged.InvokeAsync(Files) : Task.CompletedTask;

    private void Refresh()
    {
        if (!_isDisposed)
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_focusInput)
        {
            return;
        }

        _focusInput = false;

        try
        {
            if (_inputFile?.Element is { } element)
            {
                await element.FocusAsync();
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or InvalidOperationException)
        {
            // Browser or circuit is gone.
        }
    }

    // ---- text helpers

    private string SizeText(long bytes) => CyFileSize.Format(bytes, EffectiveText.UnitBytes);

    private static string ItemId(CyFileItem item) => item.Id;

    private static string ItemClass(CyFileItem item) =>
        $"cy-file-upload__item cy-file-upload__item--{item.Status.ToString().ToLowerInvariant()}";

    private string StatusText(CyFileItem item) => item.Status switch
    {
        CyFileStatus.Uploading => EffectiveText.StatusUploading,
        CyFileStatus.Uploaded => EffectiveText.StatusUploaded,
        CyFileStatus.Failed => EffectiveText.StatusFailed,
        _ => EffectiveText.StatusReady
    };

    private string ProgressLabel(CyFileItem item) => Format(EffectiveText.Progress, item.Name);

    private string CancelLabel(CyFileItem item) => Format(EffectiveText.Cancel, item.Name);

    private string RetryLabel(CyFileItem item) => Format(EffectiveText.Retry, item.Name);

    private string RemoveLabel(CyFileItem item) => Format(EffectiveText.Remove, item.Name);

    private static string Format(string format, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);

    private sealed class ImmediateProgress(Action<long> handler) : IProgress<long>
    {
        public void Report(long value) => handler(value);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _disposed.Cancel();
        _disposed.Dispose();
        GC.SuppressFinalize(this);
    }
}
