/// <summary>
/// Holds all file-system paths for the MCP server, populated from appsettings.json at startup.
/// Static so that legacy static helpers can read them without DI threading.
/// </summary>
public static class AppPaths
{
    public static string FeedFilePath      { get; set; } = "";
    public static string EventMapPath      { get; set; } = "";
    public static string QualifierMapPath  { get; set; } = "";
    public static string SchemaIndexPath   { get; set; } = "";
    public static string SchemaDocsDir     { get; set; } = "";
    public static string SystemPromptPath  { get; set; } = "";
    public static string MediaSidecarPath  { get; set; } = "";
}
