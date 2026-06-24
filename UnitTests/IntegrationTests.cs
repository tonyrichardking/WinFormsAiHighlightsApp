////////////////////////////////////////////////////////////////////////////////
//
// Integration tests — require both servers running before executing:
//   MCP server:  dotnet run --project AiHighlightsMcpServer  (port 11190)
//   Ollama/LLM:  running on localhost:11434  (or Claude via API key)
//   or cd to project directory - cd C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\WinFormsAiHighlightsApp\AiHighlightsMcpServer
//   and run:  dotnet run
//
// Run integration tests:   dotnet test --filter "TestCategory=Integration"
// Exclude from fast runs:  dotnet test --filter "TestCategory!=Integration"
//
// Note: tests that use Claude (not Ollama) consume API credit each run.
//
// cd C:\Projects\Experiments_2026\FunWithAiSoccerHighlights\WinFormsAiHighlightsApp\AiHighlightsMcpServer
// dotnet run
//
////////////////////////////////////////////////////////////////////////////////

using AiHighlightsMcpServer.Prompt_Engineering;
using OllamaMcpWebServer.Controllers;
using System.Diagnostics;
using static OllamaMcpWebServer.Controllers.AiChatController;

////////////////////////////////////////////////////////////////////////////////

namespace UnitTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class IntegrationTests
    {
        AiChatClientService theAiChatClientService = new AiChatClientService();
        AiChatController theAiChatController;

        //------------------------------------------------------------------------------
        [ClassInitialize]
        public static void ClassInitialise(TestContext testContext)
        {
            Debug.WriteLine("\nClassInitialise");
        }

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
            theAiChatController = new AiChatController(theAiChatClientService, new StubSoccerMatchInfoService());
            var modelResult = await theAiChatController.SetModel(new PutParameter { Value = "Claude" });                    // gpt-oss:latest
            var systemPromptResult = await theAiChatController.SetSystemPrompt(new PutParameter { Value = "Sports" });      
        }

        //------------------------------------------------------------------------------
        [TestCleanup]
        public void TestCleanup()
        {
            Debug.WriteLine("\nTestCleanup");
        }

        [TestMethod]
        public async Task TestMatchEventList()
        {
            string prompt = "Make a list of highlights for the match";
            string resultType = "MatchEventList";
            var actionResult = await theAiChatController.RunTypedPrompt(new TypedPromptRequest(prompt, resultType));

            // LLM responses are non-deterministic so we assert structure, not content.
            var okResult = actionResult as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.IsNotNull(okResult, "Expected an OkObjectResult from the controller");

            var responseText = okResult.Value as string;

            // check that the response is a JSON object with the expected structure and data
            if (!string.IsNullOrEmpty(responseText))
            {
                // Perform JSON structure validation here if needed
            }

            Debug.WriteLine($"\nResponse:\n{responseText}");
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
    }
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
