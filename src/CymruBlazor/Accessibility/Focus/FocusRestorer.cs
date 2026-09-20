namespace CymruBlazor.Accessibility.Focus;

/// <summary>
/// The release handle returned by the default <see cref="IFocusManager.TrapAsync"/>:
/// no containment, only "restore focus on release".
/// </summary>
internal sealed class FocusRestorer(IFocusManager manager, bool restore) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        if (restore)
        {
            await manager.RestoreFocusAsync();
        }
    }
}
