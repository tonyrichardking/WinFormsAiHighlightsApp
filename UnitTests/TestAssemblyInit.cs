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
        AppPaths.FeedFilePath      = paths["FeedFilePath"]      ?? "";
        AppPaths.EventMapPath      = paths["EventMapPath"]      ?? "";
        AppPaths.QualifierMapPath  = paths["QualifierMapPath"]  ?? "";
        AppPaths.SchemaIndexPath   = paths["SchemaIndexPath"]   ?? "";
        AppPaths.SchemaDocsDir     = paths["SchemaDocsDir"]     ?? "";
        AppPaths.SystemPromptPath  = paths["SystemPromptPath"]  ?? "";
        AppPaths.MediaSidecarPath  = paths["MediaSidecarPath"]  ?? "";
    }
}
