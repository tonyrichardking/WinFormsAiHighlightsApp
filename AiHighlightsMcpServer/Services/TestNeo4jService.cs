namespace TestServices
{
    using Microsoft.Extensions.Configuration;
    using Neo4j.Driver;
    using Serilog;

    public class TestNeo4jService : IAsyncDisposable
    {
        private readonly IDriver neo4jDriver;

        public static string TheServiceRoot { get; set; } = "bolt://localhost:7687";

        public TestNeo4jService(IConfiguration config)
        {
            //var uri = config["Neo4j:Uri"];
            //var user = config["Neo4j:User"];
            //var password = config["Neo4j:Password"];

            //neo4jDriver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));

            try
            {
                neo4jDriver = GraphDatabase.Driver(TheServiceRoot, AuthTokens.Basic("neo4j", "neo4j"));
            }
            catch (Exception ex)
            {
                Log.Error("Get Neo4j driver", ex);
            }
        }

        public async Task<List<IRecord>> RunQueryAsync(string cypherQuery, object? parameters = null)
        {
            var session = neo4jDriver.AsyncSession();

            var people = new List<IRecord>();
            try
            {
                IResultCursor cursor = await session.RunAsync(cypherQuery, parameters);

                List<IRecord> records = await cursor.ToListAsync();

                return records;





                //return await session.RunAsync(cypher, parameters);

                //return session.RunAsync(Query query, Action<TransactionConfigBuilder> action = null);

                //await Task.Delay(100); // Simulate async work
                // return "TestNeo4jService: RunQueryAsync completed";
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await neo4jDriver.DisposeAsync();
        }
    }
}
