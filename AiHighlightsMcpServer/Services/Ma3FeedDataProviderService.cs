namespace TestServices
{
    using ContentExtraction;
    using Microsoft.Extensions.Configuration;
    using Neo4j.Driver;
    using Serilog;
    using System.Text.Json;

    public class Ma3FeedDataProviderService : IAsyncDisposable
    {
        public static string TheFeedJson { get; set; }
        public static Ma3JsonClasses.Rootobject TheRootObject { get; set; }

        public Ma3FeedDataProviderService(IConfiguration config)
        {
            TheFeedJson = File.ReadAllText(AppPaths.FeedFilePath, System.Text.Encoding.UTF8);
            TheRootObject = JsonSerializer.Deserialize<Ma3JsonClasses.Rootobject>(TheFeedJson);
        }

        public async Task<Ma3JsonClasses.Rootobject> ReadMa3Objects()
        {
            return TheRootObject;
        }

        public async Task<string> ReadMa3Json()
        {
            return TheFeedJson;
        }

        public async ValueTask DisposeAsync()
        {

        }
    }
}
