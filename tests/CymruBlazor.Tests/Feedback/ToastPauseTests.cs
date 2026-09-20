using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

using CymruBlazor.Components.Feedback;

namespace CymruBlazor.Tests.Feedback;

/// <summary>
/// WCAG 2.2.1 (Timing Adjustable): a toast's countdown pauses while the toast is hovered or
/// focused and resumes when neither applies.
/// </summary>
public sealed class ToastPauseTests : TestContextBase
{
    private readonly RecordingToastService _service = new();

    public ToastPauseTests()
    {
        Services.AddSingleton<IToastService>(_service);
    }

    [Fact]
    public void Hovering_A_Toast_Pauses_It_And_Leaving_Resumes_It()
    {
        var id = _service.Add("Saved");
        var cut = Render<CyToastContainer>();

        cut.Find(".cy-toast").MouseEnter();
        _service.Calls.ShouldBe([$"pause:{id}"]);

        cut.Find(".cy-toast").MouseLeave();
        _service.Calls.ShouldBe([$"pause:{id}", $"resume:{id}"]);
    }

    [Fact]
    public void Focusing_A_Toast_Pauses_It_And_Blurring_Resumes_It()
    {
        var id = _service.Add("Saved");
        var cut = Render<CyToastContainer>();

        cut.Find(".cy-toast").FocusIn();
        cut.Find(".cy-toast").FocusOut();

        _service.Calls.ShouldBe([$"pause:{id}", $"resume:{id}"]);
    }

    [Fact]
    public void Toast_Stays_Paused_While_Either_Hover_Or_Focus_Remains()
    {
        var id = _service.Add("Saved");
        var cut = Render<CyToastContainer>();
        var toast = cut.Find(".cy-toast");

        toast.FocusIn();
        toast.MouseEnter();
        toast.MouseLeave();

        _service.Calls.ShouldNotContain($"resume:{id}");

        toast.FocusOut();

        _service.Calls.Last().ShouldBe($"resume:{id}");
    }

    [Fact]
    public void Only_The_Interacted_Toast_Is_Paused()
    {
        var first = _service.Add("One");
        _service.Add("Two");
        var cut = Render<CyToastContainer>();

        cut.FindAll(".cy-toast")[0].MouseEnter();

        _service.Calls.ShouldBe([$"pause:{first}"]);
    }

    [Fact]
    public void A_Service_Without_Pause_Support_Is_Not_Broken_By_Hover()
    {
        // 1.2.x IToastService implementations do not know the new members; the interface defaults are no-ops.
        var legacy = new LegacyToastService();
        legacy.Add("Saved");
        using var context = new BunitContext();
        context.Services.AddSingleton<CymruBlazor.Components.Core.IComponentIdGenerator, CymruBlazor.Components.Core.ComponentIdGenerator>();
        context.Services.AddSingleton<IToastService>(legacy);

        var cut = context.Render<CyToastContainer>();

        Should.NotThrow(() => cut.Find(".cy-toast").MouseEnter());
    }

    [Fact]
    public async Task ToastService_Paused_Toast_Is_Not_Removed_Until_Resumed()
    {
        using var service = new ToastService();
        service.Show("Slow", ToastVariant.Info, TimeSpan.FromMilliseconds(300));
        var id = service.Toasts[0].Id;

        service.PauseAutoDismiss(id);
        await Task.Delay(700);

        service.Toasts.Count.ShouldBe(1);

        service.ResumeAutoDismiss(id);
        await WaitUntilAsync(() => service.Toasts.Count == 0, TimeSpan.FromSeconds(4));

        service.Toasts.ShouldBeEmpty();
    }

    [Fact]
    public async Task ToastService_Resume_Gives_At_Least_One_Second()
    {
        using var service = new ToastService();
        service.Show("Short", ToastVariant.Info, TimeSpan.FromMilliseconds(200));
        var id = service.Toasts[0].Id;
        service.PauseAutoDismiss(id);
        service.ResumeAutoDismiss(id);

        await Task.Delay(500);

        service.Toasts.Count.ShouldBe(1);
    }

    [Fact]
    public void ToastService_Pause_And_Resume_Are_Safe_For_Unknown_Sticky_Or_Removed_Toasts()
    {
        using var service = new ToastService();
        service.Show("Sticky", ToastVariant.Info, TimeSpan.Zero);
        var sticky = service.Toasts[0].Id;

        Should.NotThrow(() =>
        {
            service.PauseAutoDismiss(Guid.NewGuid());
            service.ResumeAutoDismiss(Guid.NewGuid());
            service.PauseAutoDismiss(sticky);
            service.ResumeAutoDismiss(sticky);
            service.Remove(sticky);
            service.PauseAutoDismiss(sticky);
        });
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(50);
        }
    }

    private sealed class RecordingToastService : IToastService
    {
        private readonly List<ToastNotification> _toasts = [];

        public event Action? OnChange
        {
            add { }
            remove { }
        }

        public List<string> Calls { get; } = [];

        public IReadOnlyList<ToastNotification> Toasts => _toasts;

        public Guid Add(string message)
        {
            var toast = new ToastNotification(Guid.NewGuid(), message, ToastVariant.Info, TimeSpan.FromSeconds(4));
            _toasts.Add(toast);
            return toast.Id;
        }

        public void Show(string message, ToastVariant variant = ToastVariant.Success, TimeSpan? duration = null) => Add(message);

        public void Remove(Guid id) => _toasts.RemoveAll(t => t.Id == id);

        public void Clear() => _toasts.Clear();

        public void PauseAutoDismiss(Guid id) => Calls.Add($"pause:{id}");

        public void ResumeAutoDismiss(Guid id) => Calls.Add($"resume:{id}");
    }

    private sealed class LegacyToastService : IToastService
    {
        private readonly List<ToastNotification> _toasts = [];

        public event Action? OnChange
        {
            add { }
            remove { }
        }

        public IReadOnlyList<ToastNotification> Toasts => _toasts;

        public void Add(string message) =>
            _toasts.Add(new ToastNotification(Guid.NewGuid(), message, ToastVariant.Info, TimeSpan.FromSeconds(4)));

        public void Show(string message, ToastVariant variant = ToastVariant.Success, TimeSpan? duration = null) => Add(message);

        public void Remove(Guid id) => _toasts.RemoveAll(t => t.Id == id);

        public void Clear() => _toasts.Clear();
    }
}
