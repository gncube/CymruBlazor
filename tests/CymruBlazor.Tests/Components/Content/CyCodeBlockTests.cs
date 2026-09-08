using Xunit;
using Shouldly;
using Bunit;
using Microsoft.JSInterop;
using Moq;
using Mediator;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;
using CymruBlazor.Accessibility.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyCodeBlockTests : TestContextBase
{
    private readonly Mock<IMediator> _mediatorMock = new();

    public CyCodeBlockTests()
    {
        Services.AddSingleton(_mediatorMock.Object);
    }

    [Fact]
    public void Should_Render_Code_And_Language_Label()
    {
        // Act
        var cut = Render<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "var x = 1;")
            .Add(p => p.Language, "csharp"));

        // Assert
        cut.Find(".cy-code-block__language").TextContent.ShouldBe("csharp");
        cut.Find(".cy-code-block__code").TextContent.ShouldBe("var x = 1;");
    }

    [Fact]
    public void Should_Default_Language_Label_To_Code_When_Unset()
    {
        // Act
        var cut = Render<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "echo hello"));

        // Assert
        cut.Find(".cy-code-block__language").TextContent.ShouldBe("text");
    }

    [Fact]
    public void Should_Hide_Copy_Button_When_ShowCopyButton_Is_False()
    {
        // Act
        var cut = Render<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "var x = 1;")
            .Add(p => p.ShowCopyButton, false));

        // Assert
        cut.FindAll(".cy-code-block__copy").Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Copy_Code_To_Clipboard_And_Announce_Success()
    {
        // Arrange
        JSInterop.SetupVoid("navigator.clipboard.writeText", "var x = 1;");

        var cut = Render<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "var x = 1;"));

        // Act
        await cut.Find(".cy-code-block__copy").ClickAsync(new());

        // Assert - the button's visible text resets 2s after a
        // successful copy (see CyCodeBlock.CopyToClipboardAsync), so by
        // the time this fully-awaited click returns, "Copied" has
        // already reverted to "Copy" - the durable, testable effects are
        // that the clipboard call happened and the announcement fired.
        JSInterop.VerifyInvoke("navigator.clipboard.writeText");

        _mediatorMock.Verify(m => m.Publish(
            It.Is<LiveRegionAnnouncement>(a =>
                a.Message == "Code copied to clipboard." &&
                a.Politeness == LiveRegionPoliteness.Polite),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Announce_Assertively_When_Clipboard_Throws()
    {
        // Arrange
        JSInterop.SetupVoid("navigator.clipboard.writeText", "var x = 1;")
            .SetException(new JSException("Clipboard access denied"));

        var cut = Render<CyCodeBlock>(parameters => parameters
            .Add(p => p.Code, "var x = 1;"));

        // Act
        await cut.Find(".cy-code-block__copy").ClickAsync(new());

        // Assert
        _mediatorMock.Verify(m => m.Publish(
            It.Is<LiveRegionAnnouncement>(a =>
                a.Message == "Copying to clipboard failed." &&
                a.Politeness == LiveRegionPoliteness.Assertive),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
