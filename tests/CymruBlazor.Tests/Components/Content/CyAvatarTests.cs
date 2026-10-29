using Bunit;
using Shouldly;
using Xunit;
using Microsoft.AspNetCore.Components.Web;
using CymruBlazor.Components.Content;
using CymruBlazor.Enums;

namespace CymruBlazor.Tests.Components.Content;

public sealed class CyAvatarTests : TestContextBase
{
    [Theory]
    [InlineData("Gwen Davies", "GD")]
    [InlineData("gwen", "G")]
    [InlineData("Rhys ap Gruffudd", "RG")]
    [InlineData("  Ann   Marie  Jones ", "AJ")]
    [InlineData("(Dr) Sam Lee", "DL")]
    [InlineData("élan vital", "ÉV")]
    [InlineData("user.name@org.uk", "UN")]
    [InlineData("jane.doe@example.com", "JD")]
    [InlineData("first_last@domain.com", "FL")]
    [InlineData("single@domain.com", "S")]
    public void Should_Work_Out_Initials(string name, string expected)
    {
        CyAvatar.ComputeInitials(name).ShouldBe(expected);
    }

    [Fact]
    public void Should_Fall_Back_To_A_Question_Mark_When_There_Are_No_Letters()
    {
        CyAvatar.ComputeInitials("--").ShouldBe("?");
        CyAvatar.ComputeInitials(null).ShouldBe("?");
        CyAvatar.ComputeInitials("").ShouldBe("?");
    }

    [Fact]
    public void Should_Choose_The_Same_Tone_For_The_Same_Name()
    {
        CyAvatar.ToneFor("Gwen Davies").ShouldBe(CyAvatar.ToneFor("gwen davies "));
        CyAvatar.ToneFor("Gwen Davies").ShouldBeInRange(0, 5);
    }

    [Fact]
    public void Should_Generate_Deterministic_Hue()
    {
        var hue1 = CyAvatar.HueFor("Gwen Davies");
        var hue2 = CyAvatar.HueFor("gwen davies ");
        hue1.ShouldBe(hue2);
        hue1.ShouldBeInRange(0, 359);
    }

    [Fact]
    public void Should_Apply_Deterministic_Hue_In_Inline_Style()
    {
        var cut = Render<CyAvatar>(p => p.Add(c => c.Name, "Jane Doe"));
        var avatar = cut.Find(".cy-avatar");
        avatar.GetAttribute("style")!.ShouldContain("--cy-avatar-hue");
    }

    [Fact]
    public void Should_Be_Announced_As_An_Image_Named_By_The_Person()
    {
        var cut = Render<CyAvatar>(p => p.Add(c => c.Name, "Gwen Davies"));

        var avatar = cut.Find(".cy-avatar");
        avatar.GetAttribute("role").ShouldBe("img");
        avatar.GetAttribute("aria-label").ShouldBe("Gwen Davies");
        cut.Find(".cy-avatar__initials").GetAttribute("aria-hidden").ShouldBe("true");
        cut.Find(".cy-avatar__initials").TextContent.ShouldBe("GD");
    }

    [Fact]
    public void Should_Prefer_Alt_Text_For_Aria_Label_When_Supplied()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Gwen Davies")
            .Add(c => c.Alt, "Profile photo of Gwen"));

        var avatar = cut.Find(".cy-avatar");
        avatar.GetAttribute("role").ShouldBe("img");
        avatar.GetAttribute("aria-label").ShouldBe("Profile photo of Gwen");
    }

    [Fact]
    public void Should_Be_Hidden_From_Assistive_Technology_When_Decorative()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Gwen Davies")
            .Add(c => c.Decorative, true));

        var avatar = cut.Find(".cy-avatar");
        avatar.GetAttribute("aria-hidden").ShouldBe("true");
        avatar.HasAttribute("role").ShouldBeFalse();
        avatar.HasAttribute("aria-label").ShouldBeFalse();
    }

    [Fact]
    public void Should_Show_A_Photo_With_Empty_Alt_Text_Because_The_Wrapper_Is_Named()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Gwen Davies")
            .Add(c => c.ImageUrl, "/img/gwen.png"));

        var image = cut.Find("img.cy-avatar__image");
        image.GetAttribute("src").ShouldBe("/img/gwen.png");
        image.GetAttribute("alt").ShouldBe(string.Empty);
        cut.FindAll(".cy-avatar__initials").Count.ShouldBe(0);
        cut.Find(".cy-avatar").ClassList.ShouldContain("cy-avatar--image");
    }

    [Fact]
    public void Should_Fallback_To_Initials_When_Image_Fails_To_Load()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.ImageUrl, "/broken.jpg"));

        cut.FindAll("img.cy-avatar__image").Count.ShouldBe(1);
        cut.FindAll(".cy-avatar__initials").Count.ShouldBe(0);

        // Simulate image load error
        cut.Find("img.cy-avatar__image").TriggerEvent("onerror", EventArgs.Empty);

        cut.FindAll("img.cy-avatar__image").Count.ShouldBe(0);
        var initials = cut.Find(".cy-avatar__initials");
        initials.TextContent.ShouldBe("JD");
        cut.Find(".cy-avatar").ClassList.ShouldNotContain("cy-avatar--image");
    }

    [Theory]
    [InlineData(AvatarStatus.Online, "cy-avatar__status--online")]
    [InlineData(AvatarStatus.Busy, "cy-avatar__status--busy")]
    [InlineData(AvatarStatus.Away, "cy-avatar__status--away")]
    [InlineData(AvatarStatus.Offline, "cy-avatar__status--offline")]
    public void Should_Render_Status_Indicator_Dot(AvatarStatus status, string expectedClass)
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.Status, status));

        var dot = cut.Find($".cy-avatar__status.{expectedClass}");
        dot.GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Should_Not_Render_Status_Dot_When_Status_Is_None()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.Status, AvatarStatus.None));

        cut.FindAll(".cy-avatar__status").Count.ShouldBe(0);
    }

    [Fact]
    public void Avatar_Should_Instantiate_And_Render_With_AvatarStatus_Busy_Without_TypeLoadException()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Test Clinician")
            .Add(c => c.Status, AvatarStatus.Busy));

        cut.Instance.Status.ShouldBe(AvatarStatus.Busy);
        var dot = cut.Find(".cy-avatar__status--busy");
        dot.ShouldNotBeNull();
        dot.ClassList.ShouldContain("cy-avatar__status");
    }

    [Fact]
    public void AssemblyVersion_Should_Reflect_Minor_Version_2_3_0_0()
    {
        var assembly = typeof(CyAvatar).Assembly;
        var version = assembly.GetName().Version;
        version.ShouldNotBeNull();
        version.ToString().ShouldBe("2.3.0.0");
    }

    [Theory]
    [InlineData(ComponentColour.Primary, "cy-avatar--primary")]
    [InlineData(ComponentColour.Danger, "cy-avatar--danger")]
    [InlineData(ComponentColour.Success, "cy-avatar--success")]
    public void Should_Apply_Colour_Variant(ComponentColour colour, string expectedClass)
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.Colour, colour));

        cut.Find(".cy-avatar").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void Should_Render_Button_And_Trigger_OnClick_When_Handler_Supplied()
    {
        var clicked = false;
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Jane Doe")
            .Add(c => c.OnClick, (MouseEventArgs e) => clicked = true));

        var button = cut.Find("button.cy-avatar");
        button.ClassList.ShouldContain("cy-avatar--interactive");
        button.HasAttribute("role").ShouldBeFalse();
        button.GetAttribute("aria-label").ShouldBe("JD, Jane Doe");

        button.Click();
        clicked.ShouldBeTrue();
    }

    [Fact]
    public void Should_Prefer_Explicit_Initials()
    {
        var cut = Render<CyAvatar>(p => p
            .Add(c => c.Name, "Gwen Davies")
            .Add(c => c.Initials, "GJ"));

        cut.Find(".cy-avatar__initials").TextContent.ShouldBe("GJ");
    }

    [Theory]
    [InlineData(ComponentSize.ExtraSmall, "cy-avatar--xs")]
    [InlineData(ComponentSize.Small, "cy-avatar--sm")]
    [InlineData(ComponentSize.Medium, "cy-avatar--md")]
    [InlineData(ComponentSize.Unspecified, "cy-avatar--md")]
    [InlineData(ComponentSize.Large, "cy-avatar--lg")]
    [InlineData(ComponentSize.ExtraLarge, "cy-avatar--xl")]
    public void Should_Map_Size_To_A_Class(ComponentSize size, string expected)
    {
        Render<CyAvatar>(p => p.Add(c => c.Name, "A B").Add(c => c.Size, size))
            .Find(".cy-avatar").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Should_Reject_An_Empty_Name()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyAvatar>(p => p.Add(c => c.Name, " ")));
    }

    [Fact]
    public void Should_Encode_The_Name()
    {
        var cut = Render<CyAvatar>(p => p.Add(c => c.Name, "<script>x</script>"));

        cut.FindAll("script").Count.ShouldBe(0);
    }
}

public sealed class CyAvatarGroupTests : TestContextBase
{
    private static List<CyAvatarItem> People(int count) =>
        Enumerable.Range(1, count).Select(i => new CyAvatarItem($"Person {i}")).ToList();

    [Fact]
    public void Should_Render_A_List_Of_Avatars()
    {
        var cut = Render<CyAvatarGroup>(p => p.Add(c => c.Items, People(3)));

        cut.Find("ul.cy-avatar-group").ShouldNotBeNull();
        cut.FindAll("li").Count.ShouldBe(3);
        cut.FindAll("[role='img']").Count.ShouldBe(3);
    }

    [Fact]
    public void Should_Collapse_The_Rest_Into_A_Plus_Badge_Read_As_And_N_More()
    {
        var cut = Render<CyAvatarGroup>(p => p
            .Add(c => c.Items, People(7))
            .Add(c => c.Max, 4));

        cut.FindAll("li").Count.ShouldBe(5);
        var overflow = cut.Find(".cy-avatar--overflow");
        overflow.QuerySelector("[aria-hidden='true']")!.TextContent.ShouldBe("+3");
        overflow.QuerySelector(".u-sr-only")!.TextContent.ShouldBe("and 3 more");
    }

    [Fact]
    public void Should_Not_Show_A_Badge_When_Everyone_Fits()
    {
        var cut = Render<CyAvatarGroup>(p => p.Add(c => c.Items, People(4)));

        cut.FindAll(".cy-avatar--overflow").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Use_Localised_Overflow_Text()
    {
        var cut = Render<CyAvatarGroup>(p => p
            .Add(c => c.Items, People(6))
            .Add(c => c.Max, 4)
            .Add(c => c.Text, new CyAvatarGroupText { MoreFormat = "a {0} arall" }));

        cut.Find(".cy-avatar--overflow .u-sr-only").TextContent.ShouldBe("a 2 arall");
    }

    [Fact]
    public void Should_Name_The_List_When_A_Label_Is_Given()
    {
        var cut = Render<CyAvatarGroup>(p => p
            .Add(c => c.Items, People(2))
            .Add(c => c.Label, "Reviewers"));

        cut.Find("ul").GetAttribute("aria-label").ShouldBe("Reviewers");
    }

    [Fact]
    public void Should_Pass_The_Size_To_Every_Avatar()
    {
        var cut = Render<CyAvatarGroup>(p => p
            .Add(c => c.Items, People(2))
            .Add(c => c.Size, ComponentSize.Large));

        cut.FindAll(".cy-avatar").ShouldAllBe(a => a.ClassList.Contains("cy-avatar--lg"));
    }

    [Fact]
    public void Should_Reject_A_Max_Below_One()
    {
        Should.Throw<InvalidOperationException>(() => Render<CyAvatarGroup>(p => p.Add(c => c.Max, 0)));
    }

    [Fact]
    public void Should_Render_An_Empty_List_Without_Items()
    {
        Render<CyAvatarGroup>().FindAll("li").Count.ShouldBe(0);
    }
}
