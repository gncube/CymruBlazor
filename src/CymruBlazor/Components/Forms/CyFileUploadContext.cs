using Microsoft.AspNetCore.Components.Forms;

namespace CymruBlazor.Components.Forms;

/// <summary>
/// What <see cref="CyFileUpload.Upload"/> receives for one file. The library has no upload endpoint of its own: this is
/// how your code gets the bytes, sends them wherever they go, and reports progress.
/// </summary>
public sealed class CyFileUploadContext
{
    private readonly long _maxFileSize;

    internal CyFileUploadContext(IBrowserFile file, long maxFileSize, IProgress<long> progress, CancellationToken cancellationToken)
    {
        File = file;
        _maxFileSize = maxFileSize;
        Progress = progress;
        CancellationToken = cancellationToken;
    }

    /// <summary>The file as the browser describes it (name, size and type are untrusted).</summary>
    public IBrowserFile File { get; }

    /// <summary>Report the number of bytes handled so far, to move the progress bar.</summary>
    public IProgress<long> Progress { get; }

    /// <summary>Cancelled when the user cancels or removes the file, or the component is disposed.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Opens the file for reading, always with the component's <c>MaxFileSize</c> as the limit, so the framework (not just
    /// the pre-check, which trusts the browser's reported size) stops a file that turns out to be larger.
    /// </summary>
    public Stream OpenReadStream() => File.OpenReadStream(_maxFileSize, CancellationToken);

    /// <summary>Copies the file to <paramref name="destination"/>, reporting <see cref="Progress"/> as it goes.</summary>
    /// <param name="destination">Where to write the bytes.</param>
    /// <param name="bufferSize">The copy buffer size in bytes.</param>
    public async Task CopyToAsync(Stream destination, int bufferSize = 81920)
    {
        ArgumentNullException.ThrowIfNull(destination);

        await using var source = OpenReadStream();
        var buffer = new byte[bufferSize];
        long total = 0;
        int read;

        while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), CancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), CancellationToken);
            total += read;
            Progress.Report(total);
        }
    }
}

/// <summary>The outcome of <see cref="CyFileUpload.Upload"/> for one file.</summary>
public sealed class CyUploadResult
{
    private static readonly CyUploadResult SuccessInstance = new(true, null);

    private CyUploadResult(bool succeeded, string? message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    /// <summary>Whether the file was uploaded.</summary>
    public bool Succeeded { get; }

    /// <summary>The message to show when the upload failed, or <see langword="null"/> for the default.</summary>
    public string? Message { get; }

    /// <summary>The file was uploaded.</summary>
    public static CyUploadResult Success() => SuccessInstance;

    /// <summary>The file was not uploaded.</summary>
    /// <param name="message">
    /// What to tell the user ("The file contains a virus", "Not enough storage"). Plain text; never put internal detail here.
    /// When <see langword="null"/> the component's generic "could not be uploaded" text is used.
    /// </param>
    public static CyUploadResult Failure(string? message = null) => new(false, message);
}
