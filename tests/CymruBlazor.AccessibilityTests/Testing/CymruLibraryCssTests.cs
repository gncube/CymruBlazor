using CymruBlazor.Testing.Axe;
using Shouldly;
using Xunit;

namespace CymruBlazor.AccessibilityTests.Testing;

public sealed class CymruLibraryCssTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"cymru-css-{Guid.NewGuid():N}");

    public CymruLibraryCssTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Should_Read_The_Package_Layout_Beside_The_Assembly()
    {
        // Arrange: <package>/lib/net10.0/CymruBlazor.dll and <package>/staticwebassets/
        var assets = WriteAssets(Path.Combine(_root, "pkg", "staticwebassets"), main: ".a{}", scoped: ".b{}");
        var dll = Path.Combine(_root, "pkg", "lib", "net10.0", "CymruBlazor.dll");

        // Act
        var css = CymruLibraryCss.Load(null, dll, EmptyBase());

        // Assert
        assets.ShouldNotBeNull();
        css.ShouldContain(".a{}");
        css.ShouldContain(".b{}");
    }

    [Fact]
    public void Should_Use_The_Folder_It_Is_Given_Before_Searching()
    {
        // Arrange
        WriteAssets(Path.Combine(_root, "pkg", "staticwebassets"), main: ".package{}", scoped: null);
        var given = WriteAssets(Path.Combine(_root, "given"), main: ".given{}", scoped: null);
        var dll = Path.Combine(_root, "pkg", "lib", "net10.0", "CymruBlazor.dll");

        // Act
        var css = CymruLibraryCss.Load(given, dll, EmptyBase());

        // Assert
        css.ShouldContain(".given{}");
        css.ShouldNotContain(".package{}");
    }

    [Fact]
    public void Should_Fail_Loudly_When_The_Given_Folder_Has_No_Stylesheet()
    {
        // Act
        var exception = Should.Throw<FileNotFoundException>(
            () => CymruLibraryCss.Load(Path.Combine(_root, "missing"), string.Empty, EmptyBase()));

        // Assert
        exception.Message.ShouldContain("missing");
    }

    [Fact]
    public void Should_Find_A_Content_Root_Listed_In_A_Static_Web_Assets_Manifest()
    {
        // Arrange
        var assets = WriteAssets(Path.Combine(_root, "wwwroot"), main: ".manifest{}", scoped: null);
        var baseDirectory = EmptyBase();
        var manifest = System.Text.Json.JsonSerializer.Serialize(new { ContentRoots = new[] { Path.Combine(_root, "other"), assets } });
        File.WriteAllText(Path.Combine(baseDirectory, "App.staticwebassets.runtime.json"), manifest);

        // Act
        var css = CymruLibraryCss.Load(null, string.Empty, baseDirectory);

        // Assert
        css.ShouldContain(".manifest{}");
    }

    [Fact]
    public void Should_Use_The_Scoped_Bundle_Built_Beside_The_App_When_The_Folder_Has_None()
    {
        // Arrange
        var assets = WriteAssets(Path.Combine(_root, "wwwroot"), main: ".main{}", scoped: null);
        var baseDirectory = EmptyBase();
        Directory.CreateDirectory(Path.Combine(baseDirectory, "obj"));
        File.WriteAllText(Path.Combine(baseDirectory, "obj", "CymruBlazor.styles.css"), ".built{}");

        // Act
        var css = CymruLibraryCss.Load(assets, string.Empty, baseDirectory);

        // Assert
        css.ShouldContain(".main{}");
        css.ShouldContain(".built{}");
    }

    [Fact]
    public void Should_Say_Where_It_Looked_When_Nothing_Is_Found()
    {
        // Arrange
        var dll = Path.Combine(_root, "pkg", "lib", "net10.0", "CymruBlazor.dll");

        // Act
        var exception = Should.Throw<FileNotFoundException>(() => CymruLibraryCss.Load(null, dll, EmptyBase()));

        // Assert
        var message = exception.Message;
        message.ShouldContain("staticwebassets");
        message.ShouldContain(CymruLibraryCss.DirectoryVariable);
    }

    private string EmptyBase()
    {
        var path = Path.Combine(_root, $"base-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);

        return path;
    }

    private static string WriteAssets(string directory, string main, string? scoped)
    {
        Directory.CreateDirectory(Path.Combine(directory, "css"));
        File.WriteAllText(Path.Combine(directory, "css", "cymrublazor.css"), main);

        if (scoped is not null)
        {
            File.WriteAllText(Path.Combine(directory, "CymruBlazor.bundle.scp.css"), scoped);
        }

        return directory;
    }
}
