using Bunit;
using CymruBlazor.Components.Feedback;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CymruBlazor.Tests.Feedback;

public sealed class CyToastContainerTests : TestContextBase
{
    // TimeSpan.Zero means "sticky": the real ToastService never starts an
    // auto-dismiss timer, so no test here races a background Task.
    private static readonly TimeSpan Sticky = TimeSpan.Zero;

    private readonly ToastService _service = new();

    public CyToastContainerTests()
    {
        Services.AddSingleton<IToastService>(_service);
    }

    [Fact]
    public void Should_Render_Notifications_Region_And_No_Toasts_When_Empty()
    {
        var cut = Render<CyToastContainer>();

        var region = cut.Find(".cy-toast-container");
        region.GetAttribute("role").ShouldBe("region");
        region.GetAttribute("aria-label").ShouldBe("Notifications");
        cut.FindAll(".cy-toast").ShouldBeEmpty();
    }

    [Fact]
    public void Should_Render_One_Toast_Per_Notification_In_Insertion_Order()
    {
        _service.Show("First message", ToastVariant.Info, Sticky);
        _service.Show("Second message", ToastVariant.Success, Sticky);
        _service.Show("Third message", ToastVariant.Danger, Sticky);

        var cut = Render<CyToastContainer>();

        var messages = cut.FindAll(".cy-toast__message").Select(m => m.TextContent.Trim()).ToList();
        messages.ShouldBe(new[] { "First message", "Second message", "Third message" });
    }

    [Theory]
    [InlineData(ToastVariant.Info, "cy-toast--info")]
    [InlineData(ToastVariant.Success, "cy-toast--success")]
    [InlineData(ToastVariant.Warning, "cy-toast--warning")]
    [InlineData(ToastVariant.Danger, "cy-toast--danger")]
    public void Should_Apply_Variant_Class(ToastVariant variant, string expectedClass)
    {
        _service.Show("Message", variant, Sticky);

        var cut = Render<CyToastContainer>();

        cut.Find(".cy-toast").ClassList.ShouldContain(expectedClass);
    }

    [Theory]
    [InlineData(ToastVariant.Info, "status")]
    [InlineData(ToastVariant.Success, "status")]
    [InlineData(ToastVariant.Warning, "alert")]
    [InlineData(ToastVariant.Danger, "alert")]
    public void Should_Announce_With_Role_Matching_Severity(ToastVariant variant, string expectedRole)
    {
        // Mirrors CyAlert: low-severity toasts are polite status messages,
        // Warning/Danger interrupt.
        _service.Show("Message", variant, Sticky);

        var cut = Render<CyToastContainer>();

        cut.Find(".cy-toast").GetAttribute("role").ShouldBe(expectedRole);
    }

    [Fact]
    public void Should_Host_A_Polite_Non_Atomic_Live_Region()
    {
        var cut = Render<CyToastContainer>();

        var region = cut.Find(".cy-toast-container");
        region.GetAttribute("aria-live").ShouldBe("polite");
        region.GetAttribute("aria-atomic").ShouldBe("false");
    }

    [Fact]
    public void Should_Label_Close_Button_And_Remove_The_Toast_When_Clicked()
    {
        _service.Show("Dismiss me", ToastVariant.Info, Sticky);
        _service.Show("Keep me", ToastVariant.Info, Sticky);
        var cut = Render<CyToastContainer>();

        var closeButtons = cut.FindAll(".cy-toast__close");
        closeButtons.Count.ShouldBe(2);
        closeButtons[0].GetAttribute("aria-label").ShouldBe("Close notification");

        closeButtons[0].Click();

        _service.Toasts.Count.ShouldBe(1);
        _service.Toasts[0].Message.ShouldBe("Keep me");
        cut.WaitForAssertion(() =>
            cut.FindAll(".cy-toast__message").Select(m => m.TextContent.Trim()).ShouldBe(new[] { "Keep me" }));
    }

    [Fact]
    public void Should_Rerender_When_Service_Raises_OnChange_After_First_Render()
    {
        var cut = Render<CyToastContainer>();
        cut.FindAll(".cy-toast").ShouldBeEmpty();

        _service.Show("Arrived later", ToastVariant.Warning, Sticky);

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".cy-toast").Count.ShouldBe(1);
            cut.Find(".cy-toast__message").TextContent.ShouldContain("Arrived later");
        });
    }

    [Fact]
    public void Should_Unsubscribe_From_OnChange_When_Disposed()
    {
        var fake = new SubscriptionTrackingToastService();
        Services.AddSingleton<IToastService>(fake);

        var cut = Render<CyToastContainer>();
        fake.SubscriberCount.ShouldBe(1);

        ((IDisposable)cut.Instance).Dispose();

        fake.SubscriberCount.ShouldBe(0);
    }

    /// <summary>
    /// Minimal <see cref="IToastService"/> that exposes how many handlers
    /// are currently attached to <see cref="OnChange"/>.
    /// </summary>
    private sealed class SubscriptionTrackingToastService : IToastService
    {
        private Action? _onChange;

        public int SubscriberCount => _onChange?.GetInvocationList().Length ?? 0;

        public event Action? OnChange
        {
            add => _onChange += value;
            remove => _onChange -= value;
        }

        public IReadOnlyList<ToastNotification> Toasts { get; } = [];

        public void Show(string message, ToastVariant variant = ToastVariant.Success, TimeSpan? duration = null)
            => throw new NotSupportedException();

        public void Remove(Guid id) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();
    }
}
