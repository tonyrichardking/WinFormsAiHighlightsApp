using ModelContextProtocol.Server;
using Neo4j.Driver;
using System.ComponentModel;
using System.Text.Json;
using TestServices;

namespace MCPServer.MCPTools
{
    [McpServerToolType]
    public class Neo4jTestTool
    {
        private readonly TestNeo4jService testService;

        public Neo4jTestTool(TestNeo4jService service)
        {
            testService = service;
        }

        [McpServerTool(Name = "runCypherQuery"), Description("Run a Cypher query")]
        public async Task<List<IRecord>> RunCypherQueryAsync(string cypherQuery, object? parameters = null)
        {
            Console.WriteLine("\n******************************* Neo4jTestTool.RunCypherQueryAsync called *******************************");

            try
            {
                List<IRecord> result = await testService.RunQueryAsync(cypherQuery);
                return result;
            }
            catch (Exception ex)
            {
                //
                return null;
            }
        }
    }
}