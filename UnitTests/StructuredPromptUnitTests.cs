using AiHighlightsMcpServer.Prompt_Engineering;
using ContentExtraction;
using MCPServer.MCPTools;
using System.Diagnostics;
using System.Text.Json;
using TestServices;

////////////////////////////////////////////////////////////////////////////////

namespace UnitTests
{
    //------------------------------------------------------------------------------
    [TestClass]
    public class StructuredPromptUnitTests
    {
        //------------------------------------------------------------------------------
        [ClassCleanup]
        public static void ClassCleanup()
        {
            Debug.WriteLine("\nClassCleanup");
        }

        //------------------------------------------------------------------------------
        [TestInitialize]
        public void TestInitialise()
        {
            Debug.WriteLine("\nTestInitialise");
        }

        //------------------------------------------------------------------------------
        [TestCleanup]
        public void TestCleanup()
        {
            Debug.WriteLine("\nTestCleanup");
        }

        [TestMethod]
        public async Task TestStructuredPrompt()
        {
            string json = ExampleStructuredRequests.firstGoalJson;

            StructuredPromptRequest structuredRequest =
                JsonSerializer.Deserialize<StructuredPromptRequest>(json)
                ?? throw new InvalidOperationException("Empty payload.");

            string structuredPromptJson = PromptUtils.StructuredJsonToTagDelimited(structuredRequest);
        }
    }
}