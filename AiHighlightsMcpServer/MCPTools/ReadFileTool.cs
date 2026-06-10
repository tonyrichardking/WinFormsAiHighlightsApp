using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCPServer.MCPTools
{
    [McpServerToolType]
    public static class ReadFileTool
    {
        [McpServerTool(Name = "readFileContents"), Description("Read a file from the local filesystem")]
        public static async Task<string> ReadFileContents(string filepath)
        {
            Console.WriteLine("\n******************************* ReadFileTool.ReadFileContents called *******************************");

            return File.ReadAllText(filepath);
        }
    }
}