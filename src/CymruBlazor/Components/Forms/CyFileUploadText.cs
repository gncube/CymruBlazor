namespace CymruBlazor.Components.Forms;

/// <summary>
/// Every phrase <see cref="CyFileUpload"/> shows or announces, as format strings, so a Welsh (or any other) service can
/// replace them. Unset properties keep their English defaults. File names are always inserted as plain text.
/// </summary>
/// <remarks>Pass an instance to <c>Text</c>: <c>Text="@(new CyFileUploadText { ChooseFile = "Dewis ffeil" })"</c>.</remarks>
public sealed record CyFileUploadText
{
    /// <summary>The visible prompt when one file may be chosen.</summary>
    public string ChooseFile { get; init; } = "Choose file";

    /// <summary>The visible prompt when several files may be chosen.</summary>
    public string ChooseFiles { get; init; } = "Choose files";

    /// <summary>Shown beside the prompt when one file may be dropped.</summary>
    public string DropFile { get; init; } = "or drop a file here";

    /// <summary>Shown beside the prompt when several files may be dropped.</summary>
    public string DropFiles { get; init; } = "or drop files here";

    /// <summary>Limit text. {0} the largest size, e.g. "5 MB".</summary>
    public string LimitSize { get; init; } = "Each file up to {0}.";

    /// <summary>Limit text. {0} the accepted types, e.g. ".pdf, .png".</summary>
    public string LimitTypes { get; init; } = "Accepted types: {0}.";

    /// <summary>Limit text. {0} the most files.</summary>
    public string LimitCount { get; init; } = "Up to {0} files.";

    /// <summary>Limit text. {0} the largest total size.</summary>
    public string LimitTotal { get; init; } = "Up to {0} in total.";

    /// <summary>Rejection. {0} file, {1} its size, {2} the limit.</summary>
    public string TooLarge { get; init; } = "{0} is {1}. The limit is {2}.";

    /// <summary>Rejection. {0} file.</summary>
    public string Empty { get; init; } = "{0} is empty.";

    /// <summary>Rejection. {0} file, {1} the accepted types.</summary>
    public string WrongType { get; init; } = "{0} is not an accepted type. Accepted: {1}.";

    /// <summary>Rejection. {0} file, {1} the most files.</summary>
    public string TooMany { get; init; } = "{0} was not added. You can add up to {1} files.";

    /// <summary>Rejection. {0} file, {1} the total limit.</summary>
    public string TooMuch { get; init; } = "{0} was not added. The files together may not be larger than {1}.";

    /// <summary>Rejection. {0} file.</summary>
    public string Duplicate { get; init; } = "{0} has already been added.";

    /// <summary>Status word.</summary>
    public string StatusReady { get; init; } = "Ready";

    /// <summary>Status word.</summary>
    public string StatusUploading { get; init; } = "Uploading";

    /// <summary>Status word.</summary>
    public string StatusUploaded { get; init; } = "Uploaded";

    /// <summary>Status word.</summary>
    public string StatusFailed { get; init; } = "Failed";

    /// <summary>Failure message when the upload callback gave none or threw. {0} file.</summary>
    public string UploadFailed { get; init; } = "{0} could not be uploaded. Try again.";

    /// <summary>Failure message when the user cancelled. {0} file.</summary>
    public string UploadCancelled { get; init; } = "Upload of {0} was cancelled.";

    /// <summary>Accessible name of a file's progress bar. {0} file.</summary>
    public string Progress { get; init; } = "Upload progress for {0}";

    /// <summary>Accessible name of the list of files. {0} the field's label.</summary>
    public string FileList { get; init; } = "Files for {0}";

    /// <summary>Button name. {0} file.</summary>
    public string Remove { get; init; } = "Remove {0}";

    /// <summary>Button name. {0} file.</summary>
    public string Cancel { get; init; } = "Cancel upload of {0}";

    /// <summary>Button name. {0} file. Must contain <see cref="RetryButton"/> (WCAG 2.5.3, label in name).</summary>
    public string Retry { get; init; } = "Retry upload of {0}";

    /// <summary>Visible text of the remove button.</summary>
    public string RemoveButton { get; init; } = "Remove";

    /// <summary>Visible text of the cancel button.</summary>
    public string CancelButton { get; init; } = "Cancel";

    /// <summary>Visible text of the retry button.</summary>
    public string RetryButton { get; init; } = "Retry";

    /// <summary>Failure when the file turned out larger than the browser reported. {0} file, {1} the limit.</summary>
    public string TooLargeActual { get; init; } = "{0} is larger than the limit of {1}.";

    /// <summary>Rejection of a whole selection that was far too big. {0} the most files.</summary>
    public string SelectionTooLarge { get; init; } = "Too many files were selected at once. You can add up to {0} files.";

    /// <summary>Visible text of the button that starts uploading when <c>AutoUpload</c> is off.</summary>
    public string UploadButton { get; init; } = "Upload";

    /// <summary>Announcement. {0} the number of files added.</summary>
    public string AddedMany { get; init; } = "{0} files added.";

    /// <summary>Announcement. {0} file.</summary>
    public string AddedOne { get; init; } = "{0} added.";

    /// <summary>Announcement. {0} file.</summary>
    public string Uploading { get; init; } = "Uploading {0}.";

    /// <summary>Announcement. {0} file.</summary>
    public string Uploaded { get; init; } = "{0} uploaded.";

    /// <summary>Announcement. {0} file.</summary>
    public string Removed { get; init; } = "{0} removed.";

    /// <summary>Unit words, used when formatting sizes: bytes.</summary>
    public string UnitBytes { get; init; } = "bytes";
}
