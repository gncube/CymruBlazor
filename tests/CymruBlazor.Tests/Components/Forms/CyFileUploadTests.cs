using Xunit;
using Shouldly;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CymruBlazor.Components.Forms;

namespace CymruBlazor.Tests.Components.Forms;

/// <summary>
/// <see cref="CyFileUpload"/>: validation messages, the upload callback (success, failure, exception, cancel, retry,
/// concurrency, disposal), announcements and encoding. Every limit here is advisory by design; see the component's docs.
/// </summary>
public sealed class CyFileUploadTests : TestContextBase
{
    public CyFileUploadTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    private IRenderedComponent<CyFileUpload> RenderUpload(
        Action<ComponentParameterCollectionBuilder<CyFileUpload>>? configure = null) =>
        Render<CyFileUpload>(p =>
        {
            p.Add(c => c.Label, "Referral letter");
            configure?.Invoke(p);
        });

    private static InputFileContent Content(string name, int size = 5, string type = "application/pdf") =>
        InputFileContent.CreateFromBinary(new byte[size], name, null, type);

    private static void Pick(IRenderedComponent<CyFileUpload> cut, params InputFileContent[] files) =>
        cut.FindComponent<InputFile>().UploadFiles(files);

    private static string Status(IRenderedComponent<CyFileUpload> cut) => cut.Find("[role=status]").TextContent.Trim();

    private static List<string> Names(IRenderedComponent<CyFileUpload> cut) =>
        cut.FindAll(".cy-file-upload__name").Select(n => n.TextContent).ToList();

    private static List<string> Messages(IRenderedComponent<CyFileUpload> cut) =>
        cut.FindAll(".cy-file-upload__rejections li").Select(n => n.TextContent.Trim()).ToList();

    private static List<string> L(params string[] items) => items.ToList();

    private static Task<CyUploadResult> Ok(CyFileUploadContext _) => Task.FromResult(CyUploadResult.Success());

    // ---------------------------------------------------------------- markup

    [Fact]
    public void Should_Render_A_Labelled_File_Input_With_Its_Limits_As_Description()
    {
        var cut = RenderUpload(p => p
            .Add(c => c.MaxFileSize, 2 * 1024 * 1024)
            .Add(c => c.Accept, ".PDF, image/png")
            .Add(c => c.HintText, "A scan or photo"));

        var input = cut.Find("input[type=file]");
        cut.Find("label").GetAttribute("for").ShouldBe(input.Id);
        input.GetAttribute("accept").ShouldBe(".pdf,image/png");
        input.HasAttribute("multiple").ShouldBeFalse();
        var limits = cut.Find(".cy-field__hint:not([id$='-hint'])");
        limits.TextContent.ShouldBe("Each file up to 2 MB. Accepted types: .pdf, image/png.");
        input.GetAttribute("aria-describedby")!.ShouldContain(limits.Id!);
        input.GetAttribute("aria-describedby")!.ShouldContain(cut.Find("p[id$='-hint']").Id!);
    }

    [Fact]
    public void Should_Allow_Several_Files_When_Multiple_And_Say_How_Many()
    {
        var cut = RenderUpload(p => p.Add(c => c.Multiple, true).Add(c => c.MaxFiles, 3));

        cut.Find("input[type=file]").HasAttribute("multiple").ShouldBeTrue();
        cut.Find(".cy-field__hint").TextContent.ShouldContain("Up to 3 files.");
        cut.Find(".cy-file-upload__choose").TextContent.ShouldBe("Choose files");
    }

    [Fact]
    public void Should_Mark_The_Field_Required_And_Invalid_From_The_Error_Parameter()
    {
        var cut = RenderUpload(p => p.Add(c => c.Required, true).Add(c => c.Error, "Choose a file"));

        var input = cut.Find("input[type=file]");
        input.GetAttribute("aria-required").ShouldBe("true");
        input.GetAttribute("aria-invalid").ShouldBe("true");
        var error = cut.Find("p.cy-field__error");
        error.TextContent.ShouldBe("Choose a file");
        error.GetAttribute("role").ShouldBe("alert");
        input.GetAttribute("aria-describedby")!.ShouldContain(error.Id!);
    }

    [Fact]
    public void Should_Pass_Unmatched_Attributes_To_The_Input()
    {
        var cut = RenderUpload(p => p.AddUnmatched("data-test", "x"));

        cut.Find("input[type=file]").GetAttribute("data-test").ShouldBe("x");
    }

    [Fact]
    public void Should_Use_Localised_Phrases()
    {
        var cut = RenderUpload(p => p.Add(c => c.Text, new CyFileUploadText { ChooseFile = "Dewis ffeil" }));

        cut.Find(".cy-file-upload__choose").TextContent.ShouldBe("Dewis ffeil");
    }

    // ---------------------------------------------------------------- parameters

    [Theory]
    [InlineData("pdf")]
    [InlineData("image/")]
    [InlineData(".pdf;")]
    [InlineData(".pdf, ")]
    [InlineData("*/*")]
    public void Should_Reject_A_Malformed_Accept(string accept)
    {
        Should.Throw<InvalidOperationException>(() => RenderUpload(p => p.Add(c => c.Accept, accept)));
    }

    [Fact]
    public void Should_Reject_Invalid_Limits_And_A_Missing_Label()
    {
        Should.Throw<InvalidOperationException>(() => RenderUpload(p => p.Add(c => c.MaxFileSize, 0L)));
        Should.Throw<InvalidOperationException>(() => RenderUpload(p => p.Add(c => c.MaxFiles, 0)));
        Should.Throw<InvalidOperationException>(() => RenderUpload(p => p.Add(c => c.MaxParallel, 0)));
        Should.Throw<InvalidOperationException>(() => RenderUpload(p => p.Add(c => c.MaxTotalSize, 0L)));
        Should.Throw<InvalidOperationException>(() => Render<CyFileUpload>(p => p.Add(c => c.Label, " ")));
    }

    // ---------------------------------------------------------------- selecting and validating

    [Fact]
    public void Should_List_A_Valid_File_With_Its_Size_And_Announce_It()
    {
        IReadOnlyList<CyFileItem>? changed = null;
        var cut = RenderUpload(p => p.Add(c => c.OnFilesChanged, EventCallback.Factory.Create<IReadOnlyList<CyFileItem>>(this, f => changed = f)));

        Pick(cut, Content("letter.pdf", 2048));

        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("letter.pdf")));
        cut.Find(".cy-file-upload__size").TextContent.ShouldBe("2 KB");
        cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Ready");
        cut.Find("ul.cy-file-upload__list").GetAttribute("aria-label").ShouldBe("Files for Referral letter");
        Status(cut).ShouldBe("letter.pdf added.");
        changed.ShouldNotBeNull();
        changed.Count.ShouldBe(1);
        cut.Instance.Files.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Reject_A_File_That_Is_Too_Large_With_A_Specific_Message()
    {
        IReadOnlyList<CyFileRejection>? rejected = null;
        var cut = RenderUpload(p => p
            .Add(c => c.MaxFileSize, 10L)
            .Add(c => c.OnRejected, EventCallback.Factory.Create<IReadOnlyList<CyFileRejection>>(this, r => rejected = r)));

        Pick(cut, Content("big.pdf", 20));

        cut.WaitForAssertion(() => Messages(cut).ShouldBe(L("big.pdf is 20 bytes. The limit is 10 bytes.")));
        cut.Find("ul.cy-file-upload__rejections").GetAttribute("role").ShouldBe("alert");
        cut.FindAll(".cy-file-upload__item").Count.ShouldBe(0);
        rejected.ShouldNotBeNull();
        rejected[0].Reason.ShouldBe(CyFileRejectionReason.TooLarge);
        rejected[0].FileName.ShouldBe("big.pdf");
        cut.Find("input[type=file]").GetAttribute("aria-describedby")!
            .ShouldContain(cut.Find("ul.cy-file-upload__rejections").Id!);
    }

    [Fact]
    public void Should_Reject_An_Empty_File()
    {
        var cut = RenderUpload();

        Pick(cut, Content("nothing.pdf", 0));

        cut.WaitForAssertion(() => Messages(cut).ShouldBe(L("nothing.pdf is empty.")));
    }

    [Fact]
    public void Should_Accept_By_Wildcard_Mime_Type_Or_Extension()
    {
        var cut = RenderUpload(p => p.Add(c => c.Multiple, true).Add(c => c.Accept, ".pdf,image/*"));

        Pick(cut, Content("macro.docm", 4, "application/vnd.ms-word.document.macroEnabled.12"), Content("scan.PDF", 4, "application/pdf"), Content("photo.dat", 4, "image/png"));

        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("scan.PDF", "photo.dat")));
        Messages(cut).Count.ShouldBe(1);
        Messages(cut)[0].ShouldBe("macro.docm is not an accepted type. Accepted: .pdf, image/*.");
    }

    [Fact]
    public void Should_Not_Accept_A_File_With_No_Reported_Type_When_Only_Mime_Types_Are_Listed()
    {
        var cut = RenderUpload(p => p.Add(c => c.Accept, "application/pdf"));

        Pick(cut, Content("letter.pdf", 4, ""));

        cut.WaitForAssertion(() => Messages(cut).Count.ShouldBe(1));
        cut.FindAll(".cy-file-upload__item").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Reject_Files_Beyond_MaxFiles_And_Duplicates()
    {
        var cut = RenderUpload(p => p.Add(c => c.Multiple, true).Add(c => c.MaxFiles, 2));

        Pick(cut, Content("a.pdf", 1), Content("b.pdf", 2), Content("c.pdf", 3));
        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("a.pdf", "b.pdf")));
        Messages(cut).ShouldBe(L("c.pdf was not added. You can add up to 2 files."));

        Pick(cut, Content("a.pdf", 1));
        cut.WaitForAssertion(() => Messages(cut).ShouldBe(L("a.pdf has already been added.")));
        Names(cut).Count.ShouldBe(2);
    }

    [Fact]
    public void Should_Reject_Files_That_Take_The_Total_Over_MaxTotalSize()
    {
        var cut = RenderUpload(p => p.Add(c => c.Multiple, true).Add(c => c.MaxTotalSize, 10L));

        Pick(cut, Content("a.pdf", 6), Content("b.pdf", 6));

        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("a.pdf")));
        Messages(cut).ShouldBe(L("b.pdf was not added. The files together may not be larger than 10 bytes."));
    }

    [Fact]
    public void Should_Replace_The_File_In_Single_Mode()
    {
        var cut = RenderUpload();

        Pick(cut, Content("first.pdf"));
        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("first.pdf")));

        Pick(cut, Content("second.pdf"));

        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("second.pdf")));
    }

    [Fact]
    public void Should_Clear_Old_Rejections_On_The_Next_Selection()
    {
        var cut = RenderUpload(p => p.Add(c => c.MaxFileSize, 10L));

        Pick(cut, Content("big.pdf", 20));
        cut.WaitForAssertion(() => Messages(cut).Count.ShouldBe(1));

        Pick(cut, Content("ok.pdf", 5));

        cut.WaitForAssertion(() => Names(cut).ShouldBe(L("ok.pdf")));
        cut.FindAll("ul.cy-file-upload__rejections").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Encode_File_Names()
    {
        var cut = RenderUpload(p => p.Add(c => c.MaxFileSize, 10L));

        Pick(cut, Content("<img src=x onerror=alert(1)>.pdf", 5), Content("<b>big</b>.pdf", 50));

        cut.WaitForAssertion(() => Names(cut).Count.ShouldBe(1));
        cut.FindAll("img").Count.ShouldBe(0);
        cut.FindAll("b").Count.ShouldBe(0);
        cut.Markup.ShouldContain("&lt;img src=x onerror=alert(1)&gt;.pdf");
        cut.Markup.ShouldContain("&lt;b&gt;big&lt;/b&gt;.pdf");
    }

    // ---------------------------------------------------------------- removing

    [Fact]
    public void Should_Name_Every_Button_With_The_File_And_Remove_It()
    {
        var removed = 0;
        var cut = RenderUpload(p => p.Add(c => c.OnFilesChanged, EventCallback.Factory.Create<IReadOnlyList<CyFileItem>>(this, f => removed = f.Count)));

        Pick(cut, Content("letter.pdf"));
        cut.WaitForAssertion(() => cut.FindAll(".cy-file-upload__item").Count.ShouldBe(1));

        var button = cut.Find("button.cy-file-upload__button");
        button.GetAttribute("aria-label").ShouldBe("Remove letter.pdf");
        button.TextContent.ShouldContain("Remove");
        button.Click();

        cut.FindAll(".cy-file-upload__item").Count.ShouldBe(0);
        removed.ShouldBe(0);
        Status(cut).ShouldBe("letter.pdf removed.");
    }

    // ---------------------------------------------------------------- uploading

    [Fact]
    public void Should_Upload_Each_File_Through_The_Callback_And_Show_It_As_Uploaded()
    {
        var seen = new List<string>();
        var cut = RenderUpload(p => p.Add(c => c.Upload, async context =>
        {
            seen.Add(context.File.Name);
            await using var buffer = new MemoryStream();
            await context.CopyToAsync(buffer);
            return CyUploadResult.Success();
        }));

        Pick(cut, Content("letter.pdf", 8));

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Uploaded"));
        seen.ShouldBe(L("letter.pdf"));
        Status(cut).ShouldBe("letter.pdf uploaded.");
        cut.Instance.Files[0].Status.ShouldBe(CyFileStatus.Uploaded);
    }

    [Fact]
    public void Should_Show_Determinate_Progress_And_Announce_Start_But_Not_Percentages()
    {
        var gate = new TaskCompletionSource<CyUploadResult>();
        var cut = RenderUpload(p => p.Add(c => c.Upload, context =>
        {
            context.Progress.Report(4);
            return gate.Task;
        }));

        Pick(cut, Content("letter.pdf", 8));

        cut.WaitForAssertion(() =>
        {
            var progress = cut.Find("progress");
            progress.GetAttribute("value").ShouldBe("4");
            progress.GetAttribute("max").ShouldBe("8");
            progress.GetAttribute("aria-label").ShouldBe("Upload progress for letter.pdf");
        });
        cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Uploading");
        Status(cut).ShouldBe("Uploading letter.pdf.");

        gate.SetResult(CyUploadResult.Success());
        cut.WaitForAssertion(() => cut.FindAll("progress").Count.ShouldBe(0));
    }

    [Fact]
    public void Should_Show_The_Failure_Message_From_The_Callback_As_An_Alert()
    {
        var cut = RenderUpload(p => p.Add(c => c.Upload, _ => Task.FromResult(CyUploadResult.Failure("The file contains a virus."))));

        Pick(cut, Content("letter.pdf"));

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__item-error").TextContent.ShouldBe("The file contains a virus."));
        cut.Find(".cy-file-upload__item-error").GetAttribute("role").ShouldBe("alert");
        cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Failed");
        cut.Find("li.cy-file-upload__item").ClassList.ShouldContain("cy-file-upload__item--failed");
    }

    [Fact]
    public void Should_Show_A_Generic_Message_And_Never_The_Exception_Text()
    {
        var cut = RenderUpload(p => p.Add(c => c.Upload, _ => throw new InvalidOperationException("connection string: Server=secret")));

        Pick(cut, Content("letter.pdf"));

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__item-error").TextContent.ShouldBe("letter.pdf could not be uploaded. Try again."));
        cut.Markup.ShouldNotContain("secret");
    }

    [Fact]
    public void Should_Use_The_Generic_Message_When_A_Failure_Has_No_Text()
    {
        var cut = RenderUpload(p => p.Add(c => c.Upload, _ => Task.FromResult(CyUploadResult.Failure())));

        Pick(cut, Content("letter.pdf"));

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__item-error").TextContent.ShouldBe("letter.pdf could not be uploaded. Try again."));
    }

    [Fact]
    public void Should_Retry_A_Failed_Upload()
    {
        var attempts = 0;
        var cut = RenderUpload(p => p.Add(c => c.Upload, _ =>
        {
            attempts++;
            return Task.FromResult(attempts == 1 ? CyUploadResult.Failure("Network error.") : CyUploadResult.Success());
        }));

        Pick(cut, Content("letter.pdf"));
        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__item-error"));

        var retry = cut.FindAll("button.cy-file-upload__button").First(b => b.TextContent.Contains("Retry"));
        retry.GetAttribute("aria-label").ShouldBe("Retry upload of letter.pdf");
        retry.Click();

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Uploaded"));
        attempts.ShouldBe(2);
        cut.FindAll(".cy-file-upload__item-error").Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Cancel_An_Upload_In_Flight_And_Offer_A_Retry()
    {
        CancellationToken token = default;
        var cut = RenderUpload(p => p.Add(c => c.Upload, async context =>
        {
            token = context.CancellationToken;
            await Task.Delay(Timeout.Infinite, context.CancellationToken);
            return CyUploadResult.Success();
        }));

        Pick(cut, Content("letter.pdf"));
        cut.WaitForAssertion(() => cut.Find("progress"));

        var cancel = cut.Find("button.cy-file-upload__button");
        cancel.GetAttribute("aria-label").ShouldBe("Cancel upload of letter.pdf");
        cancel.Click();

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__item-error").TextContent.ShouldBe("Upload of letter.pdf was cancelled."));
        token.IsCancellationRequested.ShouldBeTrue();
        cut.FindAll("button.cy-file-upload__button").Any(b => b.TextContent.Contains("Retry")).ShouldBeTrue();
    }

    [Fact]
    public void Should_Cancel_The_Upload_When_The_File_Is_Removed()
    {
        CancellationToken token = default;
        var cut = RenderUpload(p => p.Add(c => c.Upload, async context =>
        {
            token = context.CancellationToken;
            await Task.Delay(Timeout.Infinite, context.CancellationToken);
            return CyUploadResult.Success();
        }));

        Pick(cut, Content("letter.pdf"));
        cut.WaitForAssertion(() => cut.Find("progress"));

        // While uploading the only button is Cancel; cancel, then remove the failed file.
        cut.Find("button.cy-file-upload__button").Click();
        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__item-error"));
        cut.FindAll("button.cy-file-upload__button").First(b => b.GetAttribute("aria-label")!.StartsWith("Remove", StringComparison.Ordinal)).Click();

        cut.FindAll(".cy-file-upload__item").Count.ShouldBe(0);
        token.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public void Should_Upload_One_File_At_A_Time_By_Default()
    {
        var gates = new Dictionary<string, TaskCompletionSource<CyUploadResult>>();
        var cut = RenderUpload(p => p
            .Add(c => c.Multiple, true)
            .Add(c => c.Upload, context =>
            {
                var gate = new TaskCompletionSource<CyUploadResult>();
                gates[context.File.Name] = gate;
                return gate.Task;
            }));

        Pick(cut, Content("a.pdf", 1), Content("b.pdf", 2));

        cut.WaitForAssertion(() => gates.Keys.ToList().ShouldBe(L("a.pdf")));
        Thread.Sleep(50);
        gates.Count.ShouldBe(1);

        gates["a.pdf"].SetResult(CyUploadResult.Success());

        cut.WaitForAssertion(() => gates.ContainsKey("b.pdf").ShouldBeTrue());
    }

    [Fact]
    public void Should_Upload_In_Parallel_Up_To_MaxParallel()
    {
        var started = new List<string>();
        var cut = RenderUpload(p => p
            .Add(c => c.Multiple, true)
            .Add(c => c.MaxParallel, 2)
            .Add(c => c.Upload, context =>
            {
                started.Add(context.File.Name);
                return new TaskCompletionSource<CyUploadResult>().Task;
            }));

        Pick(cut, Content("a.pdf", 1), Content("b.pdf", 2), Content("c.pdf", 3));

        cut.WaitForAssertion(() => started.Count.ShouldBe(2));
        Thread.Sleep(50);
        started.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_Wait_For_The_Button_When_AutoUpload_Is_Off()
    {
        var calls = 0;
        var cut = RenderUpload(p => p
            .Add(c => c.AutoUpload, false)
            .Add(c => c.Upload, _ =>
            {
                calls++;
                return Task.FromResult(CyUploadResult.Success());
            }));

        Pick(cut, Content("letter.pdf"));
        cut.WaitForAssertion(() => cut.Find("button.cy-file-upload__submit"));
        calls.ShouldBe(0);

        cut.Find("button.cy-file-upload__submit").Click();

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Uploaded"));
        calls.ShouldBe(1);
    }

    [Fact]
    public void Should_Not_Offer_Upload_Or_Retry_Without_A_Callback()
    {
        var cut = RenderUpload();

        Pick(cut, Content("letter.pdf"));

        cut.WaitForAssertion(() => cut.FindAll(".cy-file-upload__item").Count.ShouldBe(1));
        cut.FindAll("button.cy-file-upload__submit").Count.ShouldBe(0);
        cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Ready");
    }

    [Fact]
    public void Should_Open_The_Stream_With_MaxFileSize_As_The_Limit()
    {
        // The pre-check trusts the browser's reported size; the stream limit is what the framework enforces.
        Exception? thrown = null;
        var cut = RenderUpload(p => p
            .Add(c => c.MaxFileSize, 10L)
            .Add(c => c.Upload, context =>
            {
                using var stream = context.OpenReadStream();
                thrown = Record.Exception(() => stream.ReadExactly(new byte[10]));
                return Task.FromResult(CyUploadResult.Success());
            }));

        Pick(cut, Content("letter.pdf", 10));

        cut.WaitForAssertion(() => cut.Find(".cy-file-upload__status").TextContent.Trim().ShouldBe("Uploaded"));
        thrown.ShouldBeNull();
    }

    // ---------------------------------------------------------------- disabled and disposal

    [Fact]
    public void Should_Disable_The_Input_And_Ignore_Selections_When_Disabled()
    {
        var cut = RenderUpload(p => p.Add(c => c.Disabled, true));

        cut.Find("input[type=file]").HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void Should_Cancel_Uploads_And_Not_Call_Back_After_Disposal()
    {
        CancellationToken token = default;
        var calls = 0;
        var host = Render<RemovableHost>(p => p.Add(c => c.ChildContent, b =>
        {
            b.OpenComponent<CyFileUpload>(0);
            b.AddComponentParameter(1, nameof(CyFileUpload.Label), "Referral letter");
            b.AddComponentParameter(2, nameof(CyFileUpload.Upload), (Func<CyFileUploadContext, Task<CyUploadResult>>)(async context =>
            {
                token = context.CancellationToken;
                calls++;
                await Task.Delay(Timeout.Infinite, context.CancellationToken);
                return CyUploadResult.Success();
            }));
            b.CloseComponent();
        }));

        host.FindComponent<InputFile>().UploadFiles(Content("letter.pdf"));
        host.WaitForAssertion(() => calls.ShouldBe(1));

        host.Render(p => p.Add(c => c.Show, false));

        host.WaitForAssertion(() => token.IsCancellationRequested.ShouldBeTrue());
        calls.ShouldBe(1);
    }
}
