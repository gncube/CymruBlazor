using Microsoft.AspNetCore.Components.Forms;

namespace CymruBlazor.Components.Forms;

/// <summary>Where a file listed by <see cref="CyFileUpload"/> is in its life.</summary>
public enum CyFileStatus
{
    /// <summary>Added and waiting (to be uploaded, or to be read by your form handler when there is no upload callback).</summary>
    Ready,

    /// <summary>The upload callback is running.</summary>
    Uploading,

    /// <summary>The upload callback reported success.</summary>
    Uploaded,

    /// <summary>The upload failed or was cancelled. It can be retried when there is an upload callback.</summary>
    Failed
}

/// <summary>
/// A file the user has added to a <see cref="CyFileUpload"/>. Everything the browser reports about it
/// (<see cref="Name"/>, <see cref="Size"/>, <see cref="ContentType"/>) comes from the client and can be forged: treat it as
/// untrusted input, re-validate on the server, and never use <see cref="Name"/> as a storage path.
/// </summary>
public sealed class CyFileItem
{
    internal CyFileItem(string id, IBrowserFile file)
    {
        Id = id;
        File = file;
    }

    /// <summary>An id unique within the component, used to build element ids.</summary>
    public string Id { get; }

    /// <summary>The file as the browser describes it. Open it with <c>OpenReadStream</c> (and a size limit).</summary>
    public IBrowserFile File { get; }

    /// <summary>The name reported by the browser (untrusted).</summary>
    public string Name => File.Name;

    /// <summary>The size in bytes reported by the browser (untrusted; <c>OpenReadStream</c> enforces the real limit).</summary>
    public long Size => File.Size;

    /// <summary>The content type reported by the browser (untrusted; derived from the extension on most platforms).</summary>
    public string ContentType => File.ContentType;

    /// <summary>Where the file is in its life.</summary>
    public CyFileStatus Status { get; internal set; } = CyFileStatus.Ready;

    /// <summary>Bytes uploaded so far, as reported by the upload callback.</summary>
    public long Progress { get; internal set; }

    /// <summary>The message shown when <see cref="Status"/> is <see cref="CyFileStatus.Failed"/>.</summary>
    public string? Error { get; internal set; }

    internal bool Queued { get; set; }

    internal CancellationTokenSource? Cancellation { get; set; }
}

/// <summary>Why <see cref="CyFileUpload"/> did not add a file.</summary>
public enum CyFileRejectionReason
{
    /// <summary>Larger than <c>MaxFileSize</c>.</summary>
    TooLarge,

    /// <summary>Zero bytes.</summary>
    Empty,

    /// <summary>Not allowed by <c>Accept</c>.</summary>
    WrongType,

    /// <summary>Would exceed <c>MaxFiles</c>.</summary>
    TooMany,

    /// <summary>Would take the files over <c>MaxTotalSize</c>.</summary>
    TooMuch,

    /// <summary>A file with the same name and size is already listed.</summary>
    Duplicate
}

/// <summary>A file <see cref="CyFileUpload"/> did not add, and the message shown for it.</summary>
/// <param name="FileName">The name the browser reported (untrusted).</param>
/// <param name="Reason">Why it was not added.</param>
/// <param name="Message">The localised message shown to the user.</param>
public sealed record CyFileRejection(string FileName, CyFileRejectionReason Reason, string Message);
