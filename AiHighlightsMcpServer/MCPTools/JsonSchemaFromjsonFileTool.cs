using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MCPServer.MCPTools
{
    //[McpServerToolType]
    //public static class JsonSchemaFromjsonFileTool
    //{
    //    [McpServerTool(Name = "generateJsonSchemaFromJsonFile"), Description("Read a JSON file from the local filesystem and generate its JSON schema")]
    //    public static async Task<string> GenerateJsonSchemaFromJsonFile(string filepath)
    //    {
    //        Console.WriteLine("\n******************************* JsonSchemaFromjsonFileTool.generateJsonSchemaFromJsonFile called *******************************");

    //        string jsonString = File.ReadAllText(filepath);
    //        var schemaFromFile = JsonSchema.FromSampleJson(jsonString);
    //        var schemaData = schemaFromFile.ToJson();

    //        return schemaData;
    //    }
    //}
}