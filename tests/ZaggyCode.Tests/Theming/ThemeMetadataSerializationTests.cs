using System.Text.Json;

namespace ZaggyCode.Tests.Theming;

public class ThemeMetadataSerializationTests
{
    [Fact]
    public void Serialize_WhenMetadataSerialized_UsesKebabCaseKeys()
    {
        // Arrange
        var metadata = CreateMetadata();

        // Act
        var json = JsonSerializer.Serialize(metadata);

        // Assert
        json.Should().Contain("\"name\"");
        json.Should().Contain("\"created-at-version\"");
        json.Should().Contain("\"background-color\"");
        json.Should().Contain("\"foreground-color\"");
        json.Should().NotContain("\"BackgroundColor\"");
        json.Should().NotContain("\"CreatedAtVersion\"");
    }

    [Fact]
    public void Serialize_WhenMetadataSerialized_DoesNotIncludeRuntimeOnlyProperties()
    {
        // Arrange
        var metadata = CreateMetadata();
        metadata.IsSystemTheme = true;
        metadata.Path = "/tmp/Primus.zct";

        // Act
        var json = JsonSerializer.Serialize(metadata);

        // Assert
        json.Should().NotContain("IsSystemTheme");
        json.Should().NotContain("is-system-theme");
        json.Should().NotContain("Path");
        json.Should().NotContain("\"path\"");
    }

    [Fact]
    public void Deserialize_WhenKebabCaseJson_ReadsValues()
    {
        // Arrange
        var json = """
            {
              "name": "Primus",
              "description": "Custom",
              "created-at-version": "2026.0.0",
              "author": "TheMakarik",
              "background-color": "#101010",
              "sidebar-background-color": "#111111",
              "editor-background-color": "#121212",
              "terminal-background-color": "#131313",
              "primary-color": "#141414",
              "border-color": "#151515",
              "foreground-color": "#161616"
            }
            """;

        // Act
        var metadata = JsonSerializer.Deserialize<ThemeMetadata>(json);

        // Assert
        metadata.Should().NotBeNull();
        metadata!.Name.Should().Be("Primus");
        metadata.Description.Should().Be("Custom");
        metadata.Author.Should().Be("TheMakarik");
        metadata.CreatedAtVersion.Should().Be(new Version(2026, 0, 0));
        metadata.BackgroundColor.Should().Be("#101010");
        metadata.ForegroundColor.Should().Be("#161616");
        metadata.IsSystemTheme.Should().BeFalse();
        metadata.Path.Should().BeNull();
    }

    private static ThemeMetadata CreateMetadata() => new()
    {
        Name = "Primus",
        CreatedAtVersion = new Version(2026, 0, 0),
        Author = "TheMakarik",
        BackgroundColor = "#101010",
        SidebarBackgroundColor = "#111111",
        EditorBackgroundColor = "#121212",
        TerminalBackgroundColor = "#131313",
        PrimaryColor = "#141414",
        BorderColor = "#151515",
        ForegroundColor = "#161616"
    };
}
