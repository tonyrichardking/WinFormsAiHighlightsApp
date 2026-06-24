using ContentExtraction;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OpenAI.Assistants;
using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;
using static ContentExtraction.Ma3JsonClasses;
using static ContentExtraction.SportsFeedProcessorToolHelpers;

namespace MCPServer.MCPTools
{
    [McpServerToolType]
    public static class SportsFeedProcessorTools
    {
        //
        // public MCP tools
        //

        //[McpServerTool(Name = "readUserPrompt"), Description("Returns the original User Prompt")]
        //public static async Task<string> ReadUserPrompt()
        //{
        //    string systemPromptPath = @"C:\Projects\Experiments_2025\FunWithMCP\Ollama\OllamaMcpWebHttpDemo\OllamaMcpWebHttpDemo\SystemPrompt.txt";
        //    string currentUserPrompt = File.ReadAllText(systemPromptPath, System.Text.Encoding.UTF8);

        //    Console.WriteLine("\n******************************* ReadUserPrompt called *******************************");
        //    Console.WriteLine($"\n CurrentUserPrompt = {currentUserPrompt}");

        //    return $"User Prompt: {currentUserPrompt}";
        //}

        [McpServerTool(Name = "readEventMap"), Description("returns the Json file that contains all the event typeIds, names, descriptions, and associated qualifiers")]
        public static async Task<string> ReadEventMap()
        {
            string jsonString = SportsFeedProcessorToolHelpers.readEventMap();

            Console.WriteLine($"\n******************************* SportsFeedProcessorTools.ReadEventMap called *******************************");
            Console.WriteLine($"\n Length = {jsonString.Length}");

            return jsonString;
        }

        [McpServerTool(Name = "readEventAttributesForTypeId"), Description("returns the Event Attributes for an event typeId as textual Json")]
        public static async Task<string> ReadEventAttributesForTypeId(int typeId)
        {
            string jsonString = SportsFeedProcessorToolHelpers.readEventAttributesForTypeId(typeId);

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadEventAttributesForTypeId called *******************************");
            Console.WriteLine($"\n Parameter typeId = {typeId}, Length = {jsonString.Length}");

            return jsonString;
        }

        [McpServerTool(Name = "readQualifierMap"), Description("returns the Json file that contains all the qualifierIds, names, descriptions, and associated events")]
        public static async Task<string> ReadQualifierMap()
        {
            string jsonString = SportsFeedProcessorToolHelpers.readQualifierMap();

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadQualifierMap called *******************************");
            Console.WriteLine($"\n Length = {jsonString.Length}");

            return jsonString;
        }

        [McpServerTool(Name = "readQualifierAttributesForQualifierId"), Description("returns the Qualifier Attributes for a qualifierId as textual Json")]
        public static async Task<string> ReadQualifierAttributesForQualifierId(int qualifierId)
        {
            string jsonString = SportsFeedProcessorToolHelpers.readQualifierAttributesForQualifierId(qualifierId);

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadQualifierAttributesForQualifierId called *******************************");
            Console.WriteLine($"\n Parameter qualifierId = {qualifierId},  Length = {jsonString.Length}");

            return jsonString;
        }

        [McpServerTool(Name = "readSchemaIndex"), Description("returns the Schema Index as textual Json")]
        public static async Task<string> ReadSchemaIndex()
        {
            string jsonString = SportsFeedProcessorToolHelpers.readSchemaIndex();

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadSchemaIndex called *******************************");
            Console.WriteLine($"\n Length = {jsonString.Length}");

            return jsonString;
        }

        [McpServerTool(Name = "readSchemaDocument"), Description("takes a Nesting value string in the form 'root-level1-level2-...' and returns the corresponding Schema Document as textual Json.")]
        public static async Task<string> ReadSchemaDocument(string nesting)
        {
            string jsonString = SportsFeedProcessorToolHelpers.readSchemaDocumentForNesting(nesting);

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadSchemaDocument called *******************************");
            Console.WriteLine($"\n Parameter nesting = {nesting}, Length = {jsonString.Length}");

            return jsonString;
        }

        [McpServerTool(Name = "probeSportFeed"), Description("takes a Nesting value string in the form 'root-level1-level2-...' " +
            "and returns a message for the AI assistant indicating whether or not filtering is required for the call to ReadSportFeed.")]
        public static async Task<object> ProbeSportFeed(string nesting)
        {
            List<Dictionary<string, object>> result = null;

            // TODO: don't want these baked into the code
            if (nesting == "root-liveData" || nesting == "root-liveData-event_items" || nesting == "root-liveData-event-qualifier_items")
            {
                result = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "Message to AI assistant", $"The nesting \"{nesting}\" returns a large result without filtering.  You MUST use bindings to reduce the size of the result." }
                        }
                    };
            }
            else
            {
                result = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "Message to AI assistant", $"The nesting \"{nesting}\" does not need filtering.  Do not use bindings" }
                        }
                    };
            }

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ProbeSportFeed called *******************************");
            Console.WriteLine($"\n Parameter nesting = {nesting}");

            return result;
        }

        [McpServerTool(Name = "readSportFeed"), Description("Takes as parameters " +
            "1. a Nesting value string in the form 'level1-level2-...',  " +
            "2. (optional, default \"\") a JSON string encoding a Dictionary<string, object> of attribute name/value pairs to filter by, e.g. \"{\\\"typeId\\\": \\\"16\\\"}\" " +
            "3. (optional, default \"\") a JSON string encoding a string array of attribute names to return, e.g. \"[\\\"typeId\\\",\\\"timeMin\\\"]\" " +
            "and returns either a list of Dictionary<string, object> that contain the wanted attributes, " +
            "or a message for the AI assistant indicating that filtering is required for large results.")]
        public static async Task<object> ReadSportFeed(string nesting, string bindingsJson = "", string wantedAttributesJson = "")
        {

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadSportFeed called *******************************");
            Console.WriteLine($"\n Parameter nesting = {nesting}");

            // Empty string means no filter (equivalent to the previous null defaults).
            Dictionary<string, object>? bindings = string.IsNullOrWhiteSpace(bindingsJson)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object>>(bindingsJson);

            List<string>? wantedAttributes = string.IsNullOrWhiteSpace(wantedAttributesJson)
                ? null
                : JsonSerializer.Deserialize<List<string>>(wantedAttributesJson);

            // TODO: don't want these baked into the code
            if (bindings == null && (nesting == "root-liveData" || nesting == "root-liveData-event_items" || nesting == "root-liveData-event-qualifier_items"))
            {
                Console.WriteLine($"The nesting \"{nesting}\" returns a large result without filtering.  You MUST use bindings to reduce the size of the result.");
                return new Dictionary<string, object>
                    {
                        { "Message to AI assistant", $"The nesting \"{nesting}\" returns a large result without filtering.  You MUST use bindings to reduce the size of the result." }
                    };
            }

            string jsonString = SportsFeedProcessorToolHelpers.readSportFeedForNesting(nesting);
            List<Dictionary<string, object>> deserializedObjects = SportsFeedProcessorToolHelpers.FilterHelpers.DeserializeToRecords(jsonString);
            List<Dictionary<string, object>>? result = SportsFeedProcessorToolHelpers.FilterHelpers.FilterByAttribute(deserializedObjects, bindings, wantedAttributes, 900);

            string bindingsMessage        = string.IsNullOrWhiteSpace(bindingsJson)        ? "null" : bindingsJson;
            string wantedAttributesMessage = string.IsNullOrWhiteSpace(wantedAttributesJson) ? "null" : wantedAttributesJson;

            Console.WriteLine($"\n Parameter nesting = {nesting}, Item Count = {result?.Count}, " +
                $"bindings = {bindingsMessage}, " +
                $"wantedAttributes = {wantedAttributesMessage}");

            return result;
        }

        // ******************************************************************************

        [McpServerTool(Name = "readSportFeedItem"), Description("Takes a Nesting value string in the form 'level1-level2-...' and an id value, and returns the corresponding single item of an array section of the sports feed as textual Json.")]
        public static async Task<string> ReadSportFeedItem(string nesting, object id)
        {
            string jsonString = SportsFeedProcessorToolHelpers.readSportFeedForNestingAndId(nesting, id);

            Console.WriteLine("\n******************************* SportsFeedProcessorTools.ReadSportFeedItem called *******************************");
            Console.WriteLine($"\n Parameter nesting = {nesting}, id = {id}, Length = {jsonString.Length}");

            return jsonString;
        }
    }
}

