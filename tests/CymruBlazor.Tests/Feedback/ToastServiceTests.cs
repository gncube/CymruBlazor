namespace CymruBlazor.Tests.Feedback;

using CymruBlazor.Components.Feedback;
using Shouldly;
using Xunit;

public sealed class ToastServiceTests
{
    [Fact]
    public void WhenShowIsInvokedNotificationIsAddedPreservingInsertionOrder()
    {
        using var service = new ToastService();

        service.Show("First message", ToastVariant.Info);
        service.Show("Second message", ToastVariant.Success);

        service.Toasts.Count.ShouldBe(2);
        service.Toasts[0].Message.ShouldBe("First message");
        service.Toasts[0].Variant.ShouldBe(ToastVariant.Info);
        service.Toasts[1].Message.ShouldBe("Second message");
        service.Toasts[1].Variant.ShouldBe(ToastVariant.Success);
    }

    [Fact]
    public void WhenMessageIsNullOrWhitespaceShowThrowsArgumentException()
    {
        using var service = new ToastService();

        Should.Throw<ArgumentException>(() => service.Show("   "));
    }

    [Fact]
    public void WhenDurationIsNegativeShowThrowsArgumentOutOfRangeException()
    {
        using var service = new ToastService();

        Should.Throw<ArgumentOutOfRangeException>(() => service.Show("Valid", duration: TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void WhenRemoveIsCalledToastIsRemovedById()
    {
        using var service = new ToastService();
        service.Show("Item to delete");
        var toastId = service.Toasts[0].Id;

        service.Remove(toastId);

        service.Toasts.ShouldBeEmpty();
    }

    [Fact]
    public void WhenCapacityExceedsMaxOldestToastIsEvicted()
    {
        using var service = new ToastService();

        for (var i = 1; i <= 6; i++)
        {
            service.Show($"Message {i}");
        }

        service.Toasts.Count.ShouldBe(5);
        service.Toasts[0].Message.ShouldBe("Message 2");
        service.Toasts[4].Message.ShouldBe("Message 6");
    }
}
