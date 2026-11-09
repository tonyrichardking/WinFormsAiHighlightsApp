using Microsoft.Extensions.Configuration;

namespace UnitTests;

[TestClass]
public sealed class TestAssemblyInit
{
    [AssemblyInitialize]
    public static void Initialize(TestContext _)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var paths = config.GetSection("Paths");
        var appDir        = paths["AppDir"]        ?? "";
        var solutionDir   = paths["SolutionDir"]   ?? "";
        var mediaDir      = paths["MediaDir"]      ?? "";
        var schemaMapsDir = paths["SchemaMapsDir"] ?? "";
        var schemaDocsDir = paths["SchemaDocsDir"] ?? "";

        AppPaths.FeedFilePath     = Path.Combine(appDir, mediaDir,      paths["FeedFileName"]        ?? "");
        AppPaths.MediaSidecarPath = Path.Combine(appDir, mediaDir,      paths["SidecarFileName"]     ?? "");
        AppPaths.EventMapPath     = Path.Combine(appDir, schemaMapsDir, "EventMap.json");
        AppPaths.QualifierMapPath = Path.Combine(appDir, schemaMapsDir, "QualifierMap.json");
        AppPaths.SchemaIndexPath  = Path.Combine(appDir, schemaMapsDir, "SchemaIndex.json");
        AppPaths.SchemaDocsDir    = Path.Combine(appDir, schemaDocsDir);
        AppPaths.SystemPromptPath = Path.Combine(appDir, solutionDir,   paths["SystemPromptRelPath"] ?? "");
    }
}
