// Генератор стандартных тем Zaggy's Code (.zct = tar.bz2 с meta.json + theme.xml).
// Запуск из корня репозитория: dotnet run dev/scripts/generate-themes.cs
// Готовые архивы складываются в dev/themes — дальше их переносят вручную
// в src/desktop/ZaggyCode.Avalonia/Themes/.
#:project ../../src/desktop/ZaggyCode.Core/ZaggyCode.Core.csproj
#:project ../../src/desktop/ZaggyCode.Modules/ZaggyCode.Modules.csproj
#:property JsonSerializerIsReflectionEnabledByDefault=true

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ZaggyCode.Core.Archiving.Interfaces;
using ZaggyCode.Core.Common.Utils;
using ZaggyCode.Core.Data.Interfaces;
using ZaggyCode.Core.Theming.Model;
using ZaggyCode.Core.Theming.Interfaces;
using ZaggyCode.Modules.Archiving;
using ZaggyCode.Modules.Archiving.Options;
using ZaggyCode.Modules.Data;
using ZaggyCode.Modules.Theming;
using ZaggyCode.Modules.Theming.Options;

const string Author = "TheMakarik";
var version = new Version(2026, 0, 0);
var outputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "dev", "themes"));

var tempRoot = Path.Join(Path.GetTempPath(), "zaggy-theme-generator");
var tempOptions = new TempOptions
{
    TempDirectoryPath = tempRoot,
    TempToCompress = Path.Join(tempRoot, "to-compress"),
    TempFromCompress = Path.Join(tempRoot, "from-compress")
};

IArchiveCompressor compressor = new TarBZip2ArchiveCompressor(NullLogger<TarBZip2ArchiveCompressor>.Instance, Options.Create(tempOptions));
ITempFolderProvider tempFolderProvider = new TempFolderProvider(Options.Create(tempOptions));
IThemeCreator creator = new ThemeCreator(
    NullLogger<ThemeCreator>.Instance,
    compressor,
    tempFolderProvider,
    Options.Create(new ThemeOptions
    {
        ThemeExtensions = ".zct",
        ThemeFileName = "theme.xml",
        SystemThemesFolder = outputDirectory,
        ExternThemesFolder = outputDirectory
    }),
    Options.Create(new MetadataOptions { MetadataFile = "meta.json" }),
    new XmlSerializer<Theme>());

Theme CreateTheme(
    string background, string backgroundSecondary, string surface, string surfaceLight, string surfaceHover,
    string primary, string primaryLight, string primaryDark,
    string foreground, string foregroundMuted, string foregroundDark,
    string border, string borderLight,
    string success, string successDark, string error, string warning,
    string editorBackground, string editorForeground, string editorLineNumber,
    string terminalBackground, string terminalForeground,
    string sidebar,
    string systemAccent, string systemAccentLight1, string systemAccentDark1,
    string mapWall, string mapPointBorder, string mapPointBackground) => new()
{
    BackgroundColor = background,
    BackgroundSecondaryColor = backgroundSecondary,
    SurfaceColor = surface,
    SurfaceLightColor = surfaceLight,
    SurfaceHoverColor = surfaceHover,
    PrimaryColor = primary,
    PrimaryLightColor = primaryLight,
    PrimaryDarkColor = primaryDark,
    ForegroundColor = foreground,
    ForegroundMutedColor = foregroundMuted,
    ForegroundDarkColor = foregroundDark,
    BorderColor = border,
    BorderLightColor = borderLight,
    SuccessColor = success,
    SuccessDarkColor = successDark,
    ErrorColor = error,
    WarningColor = warning,
    EditorBackgroundColor = editorBackground,
    EditorForegroundColor = editorForeground,
    EditorLineNumberColor = editorLineNumber,
    TerminalBackgroundColor = terminalBackground,
    TerminalForegroundColor = terminalForeground,
    SidebarBackgroundColor = sidebar,
    SystemAccentColor = systemAccent,
    SystemAccentColorLight1 = systemAccentLight1,
    SystemAccentColorDark1 = systemAccentDark1,
    MapWallColor = mapWall,
    MapPointBorderColor = mapPointBorder,
    MapPointBackgroundColor = mapPointBackground
};

ThemeMetadata CreateMetadata(string name, string description, Theme theme) => new()
{
    Name = name,
    Description = description,
    CreatedAtVersion = version,
    Author = Author,
    BackgroundColor = theme.BackgroundColor,
    SidebarBackgroundColor = theme.SidebarBackgroundColor,
    EditorBackgroundColor = theme.EditorBackgroundColor,
    TerminalBackgroundColor = theme.TerminalBackgroundColor,
    PrimaryColor = theme.PrimaryColor,
    BorderColor = theme.BorderColor,
    ForegroundColor = theme.ForegroundColor
};

var primus = CreateTheme(
    background: "#1A1A1A", backgroundSecondary: "#242424", surface: "#2E2E2E", surfaceLight: "#383838", surfaceHover: "#444444",
    primary: "#8A9A7A", primaryLight: "#A8BC9A", primaryDark: "#6A7A5A",
    foreground: "#F0F0F0", foregroundMuted: "#A8A8A8", foregroundDark: "#707070",
    border: "#3D3D3D", borderLight: "#505050",
    success: "#6ABA6A", successDark: "#4AA84A", error: "#D87060", warning: "#E8B840",
    editorBackground: "#161616", editorForeground: "#F0F0F0", editorLineNumber: "#707070",
    terminalBackground: "#1A1A1A", terminalForeground: "#F0F0F0",
    sidebar: "#242424",
    systemAccent: "#8A9A7A", systemAccentLight1: "#A8BC9A", systemAccentDark1: "#6A7A5A",
    mapWall: "#8A8F96", mapPointBorder: "#F0F0F0", mapPointBackground: "#383838");

var tero = CreateTheme(
    background: "#2B211B", backgroundSecondary: "#352922", surface: "#403329", surfaceLight: "#4C3D30", surfaceHover: "#594838",
    primary: "#C97B4A", primaryLight: "#E09A6A", primaryDark: "#A05E36",
    foreground: "#F2E9DF", foregroundMuted: "#C4B5A5", foregroundDark: "#97897B",
    border: "#4A3B30", borderLight: "#61503F",
    success: "#8FAF6E", successDark: "#75955A", error: "#C96A55", warning: "#D9A441",
    editorBackground: "#241C16", editorForeground: "#F2E9DF", editorLineNumber: "#97897B",
    terminalBackground: "#261E18", terminalForeground: "#F2E9DF",
    sidebar: "#352922",
    systemAccent: "#C97B4A", systemAccentLight1: "#E09A6A", systemAccentDark1: "#A05E36",
    mapWall: "#7A6650", mapPointBorder: "#F2E9DF", mapPointBackground: "#4C3D30");

var viola = CreateTheme(
    background: "#232130", backgroundSecondary: "#2A2839", surface: "#322F44", surfaceLight: "#3B3850", surfaceHover: "#45415E",
    primary: "#9B84D9", primaryLight: "#B7A5E8", primaryDark: "#7A63B8",
    foreground: "#EDEAF5", foregroundMuted: "#ABA5C4", foregroundDark: "#7E7899",
    border: "#3D3954", borderLight: "#514C6E",
    success: "#7FBF7F", successDark: "#63A863", error: "#D87A94", warning: "#E0B458",
    editorBackground: "#1B1A26", editorForeground: "#EDEAF5", editorLineNumber: "#7E7899",
    terminalBackground: "#201E2C", terminalForeground: "#EDEAF5",
    sidebar: "#2A2839",
    systemAccent: "#9B84D9", systemAccentLight1: "#B7A5E8", systemAccentDark1: "#7A63B8",
    mapWall: "#6E6890", mapPointBorder: "#EDEAF5", mapPointBackground: "#45415E");

var brilla = CreateTheme(
    background: "#12262B", backgroundSecondary: "#17343B", surface: "#1D424B", surfaceLight: "#255059", surfaceHover: "#2D5F6A",
    primary: "#B59664", primaryLight: "#D2BA8C", primaryDark: "#8F7550",
    foreground: "#EFF7F8", foregroundMuted: "#A9C9CE", foregroundDark: "#6E99A0",
    border: "#22454E", borderLight: "#34606A",
    success: "#52C352", successDark: "#3FA83F", error: "#D73F3F", warning: "#E8B840",
    editorBackground: "#0D1D21", editorForeground: "#EFF7F8", editorLineNumber: "#6E99A0",
    terminalBackground: "#12262B", terminalForeground: "#EFF7F8",
    sidebar: "#17343B",
    systemAccent: "#17A2B8", systemAccentLight1: "#37BDD2", systemAccentDark1: "#106E7D",
    mapWall: "#106E7D", mapPointBorder: "#EFF7F8", mapPointBackground: "#255059");

(string Name, string Description, Theme Theme)[] themes =
[
    ("Primus", "Стандартная тема для Zaggy's Code - темная, с ноткой желтого", primus),
    ("Tero", "Тема в земляных тонах - терракота, охра и тёплый коричневый природной палитры", tero),
    ("Viola", "Фиолетовая тема в духе фирменных цветов .NET - мягкие оттенки фиолетового, которые не режут глаз", viola),
    ("Brilla", "Тема в стиле маскота Загги - тёплые золотые акценты на глубоких морских оттенках голубого", brilla)
];

foreach (var (name, description, theme) in themes)
    await creator.CreateAsync(theme, CreateMetadata(name, description, theme), outputDirectory);

Console.WriteLine($"Generated {themes.Length} themes into {outputDirectory}");




