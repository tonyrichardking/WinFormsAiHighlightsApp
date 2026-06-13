////////////////////////////////////////////////////////////////////////////////
//
// Integration tests — require both servers running before executing:
//   MCP server:  dotnet run --project AiHighlightsMcpServer  (port 11190)
//   Ollama/LLM:  running on localhost:11434  (or Claude via API key)
//
// Run integration tests:   dotnet test --filter "TestCategory=Integration"
// Exclude from fast runs:  dotnet test --filter "TestCategory!=Integration"
//
// Note: tests that use Claude (not Ollama) consume API credit each run.
//
////////////////////////////////////////////////////////////////////////////////

using AiHighlightsWinFormsUi;
using OllamaMcpWebServer.Controllers;
using System.Diagnostics;
using System.Text.Json;
using static OllamaMcpWebServer.Controllers.AiChatController;

////////////////////////////////////////////////////////////////////////////////

namespace UnitTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class SystemPromptUnitTests
    {
        AiChatClientService theAiChatClientService = new AiChatClientService();
        AiChatController theAiChatController;

        //------------------------------------------------------------------------------
        [ClassCleanup]
        public static void ClassCleanup()
        {
            Debug.WriteLine("\nClassCleanup");
        }

        //------------------------------------------------------------------------------
        [TestInitialize]
        public async Task TestInitialise()
        {
            Debug.WriteLine("\nTestInitialise");

            // Skip gracefully if the MCP server isn't running rather than failing with
            // a raw connection error that obscures the real cause.
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            try
            {
                await http.GetAsync("http://localhost:11190/aiChat/getOptions");
            }
            catch
            {
                Assert.Inconclusive("MCP server not reachable on :11190 — start it before running integration tests.");
            }


            await theAiChatClientService.InitialiseApi();
            theAiChatController = new AiChatController(theAiChatClientService);
            var modelResult = await theAiChatController.SetModel(new PutParameter { Value = "Claude" });     // gpt-oss:latest
        }

        //------------------------------------------------------------------------------
        [TestCleanup]
        public void TestCleanup()
        {
            Debug.WriteLine("\nTestCleanup");
        }

        [TestMethod]
        public async Task TestTextPrompt()
        {
            var actionResult = await theAiChatController.RunPrompt("Hello.  Please introduce yourself");

            // LLM responses are non-deterministic so we assert structure, not content.
            var okResult = actionResult as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.IsNotNull(okResult, "Expected an OkObjectResult from the controller");

            var responseText = okResult.Value as string;
            Assert.IsFalse(string.IsNullOrWhiteSpace(responseText),
                "Expected a non-empty text response from the model");

            Debug.WriteLine($"\nResponse:\n{responseText}");
        }

        [TestMethod]
        public async Task TestStructuredPrompt()
        {
            string json = ExampleStructuredRequestJson.json;

            StructuredPromptRequest structuredRequest =
                JsonSerializer.Deserialize<StructuredPromptRequest>(json)
                ?? throw new InvalidOperationException("Empty payload.");

            var actionResult = await theAiChatController.RunStructuredPrompt(structuredRequest);

            // LLM responses are non-deterministic so we assert structure, not content.
            var okResult = actionResult as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.IsNotNull(okResult, "Expected an OkObjectResult from the controller");

            var responseText = okResult.Value as string;
            Assert.IsFalse(string.IsNullOrWhiteSpace(responseText),
                "Expected a non-empty text response from the model");

            Debug.WriteLine($"\nResponse:\n{responseText}");
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
