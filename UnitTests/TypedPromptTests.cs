using AiHighlightsMcpServer.Prompt_Engineering;
using AiHighlightsMcpServer.Services;
using Microsoft.AspNetCore.Mvc;
using OllamaMcpWebServer.Controllers;
using System.Text.Json;
using static OllamaMcpWebServer.Controllers.AiChatController;

namespace UnitTests;

// =============================================================================
// TypedPromptRequest DTO tests — no server or LLM required.
// =============================================================================

[TestClass]
public class TypedPromptRequestDtoTests
{
    [TestMethod]
    public void Deserializes_From_Json()
    {
        var json = """{"prompt":"Who scored the first goal?","resultType":"GoalScorer"}""";
        var req  = JsonSerializer.Deserialize<TypedPromptRequest>(json);

        Assert.IsNotNull(req);
        Assert.AreEqual("Who scored the first goal?", req.Prompt);
        Assert.AreEqual("GoalScorer", req.ResultType);
    }

    [TestMethod]
    public void RoundTrip_Preserves_Values()
    {
        var original  = new TypedPromptRequest("find all fouls", "MatchEventList");
        var json      = JsonSerializer.Serialize(original);
        var restored  = JsonSerializer.Deserialize<TypedPromptRequest>(json)!;

        Assert.AreEqual(original.Prompt,     restored.Prompt);
        Assert.AreEqual(original.ResultType, restored.ResultType);
    }
}

// =============================================================================
// SoccerResultTypeCatalog tests — no server or LLM required.
// =============================================================================

[TestClass]
public class SoccerResultTypeCatalogTests
{
    [TestMethod]
    public void Catalog_IsNotEmpty()
    {
        Assert.IsTrue(SoccerResultTypeCatalog.Descriptions.Count > 0);
    }

    [TestMethod]
    public void Catalog_ContainsExpectedTypes()
    {
        var expected = new[]
        {
            "MatchEvent", "MatchEventList",
            "PlayerAppearance", "PlayerList",
        };

        foreach (var name in expected)
            Assert.IsTrue(SoccerResultTypeCatalog.Descriptions.ContainsKey(name),
                $"Catalog missing entry for '{name}'");
    }

    [TestMethod]
    public void Catalog_AllDescriptions_AreNonEmpty()
    {
        foreach (var (key, desc) in SoccerResultTypeCatalog.Descriptions)
            Assert.IsFalse(string.IsNullOrWhiteSpace(desc),
                $"Description for '{key}' is empty");
    }
}

// =============================================================================
// Controller unit tests — stub service, no server or LLM required.
// =============================================================================

[TestClass]
public class TypedPromptControllerTests
{
    // -------------------------------------------------------------------------
    // getResultTypes
    // -------------------------------------------------------------------------

    [TestMethod]
    public void GetResultTypes_ReturnsOk()
    {
        var (controller, _) = MakeController();
        var result = controller.GetResultTypes();

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public void GetResultTypes_ReturnsDictionary_WithAllTypes()
    {
        var (controller, _) = MakeController();
        var ok   = (OkObjectResult)controller.GetResultTypes();
        var dict = ok.Value as Dictionary<string, string>;

        Assert.IsNotNull(dict);
        Assert.IsTrue(dict.ContainsKey("MatchEventList"));
        Assert.IsTrue(dict.ContainsKey("PlayerList"));
    }

    // -------------------------------------------------------------------------
    // runTypedPrompt — known types return Ok
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task RunTypedPrompt_MatchEvent_CallsFindEventAsync()
        => await AssertDispatch("MatchEvent", nameof(StubSoccerMatchInfoService.FindEventAsync));

    [TestMethod]
    public async Task RunTypedPrompt_MatchEventList_CallsFindEventsAsync()
        => await AssertDispatch("MatchEventList", nameof(StubSoccerMatchInfoService.FindEventsAsync));

    [TestMethod]
    public async Task RunTypedPrompt_PlayerAppearance_CallsFindPlayerAsync()
        => await AssertDispatch("PlayerAppearance", nameof(StubSoccerMatchInfoService.FindPlayerAsync));

    [TestMethod]
    public async Task RunTypedPrompt_PlayerList_CallsFindPlayersAsync()
        => await AssertDispatch("PlayerList", nameof(StubSoccerMatchInfoService.FindPlayersAsync));

    // -------------------------------------------------------------------------
    // runTypedPrompt — unknown type returns BadRequest
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task RunTypedPrompt_UnknownType_ReturnsBadRequest()
    {
        var (controller, _) = MakeController();
        var request         = new TypedPromptRequest("any prompt", "UnknownType");

        var result = await controller.RunTypedPrompt(request);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result,
            "An unknown resultType should return 400 Bad Request");
    }

    [TestMethod]
    public async Task RunTypedPrompt_UnknownType_ResponseContainsValidTypes()
    {
        var (controller, _) = MakeController();
        var result          = await controller.RunTypedPrompt(new TypedPromptRequest("p", "BadType"))
                              as BadRequestObjectResult;

        var json = JsonSerializer.Serialize(result!.Value);
        Assert.IsTrue(json.Contains("validTypes"), "Bad-request body should list validTypes");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static (AiChatController controller, StubSoccerMatchInfoService stub) MakeController()
    {
        var stub = new StubSoccerMatchInfoService();
        return (new AiChatController(new StubTypedAiChatClientService(), stub), stub);
    }

    private static async Task AssertDispatch(string resultType, string expectedMethod)
    {
        var (controller, stub) = MakeController();
        var request = new TypedPromptRequest("test prompt", resultType);

        var result = await controller.RunTypedPrompt(request);

        Assert.IsInstanceOfType<OkObjectResult>(result,
            $"ResultType '{resultType}' should return 200 OK");
        Assert.AreEqual(expectedMethod, stub.LastMethod,
            $"ResultType '{resultType}' should dispatch to {expectedMethod}");
        Assert.AreEqual("test prompt", stub.LastPrompt,
            "Prompt should be forwarded unchanged to the service");

        var json = ((OkObjectResult)result).Value as string;
        Assert.IsFalse(string.IsNullOrWhiteSpace(json) || json == "null",
            $"ResultType '{resultType}' response body should not be null");
    }
}

// =============================================================================
// Integration test — requires live MCP server on :11190.
// =============================================================================

[TestClass]
[TestCategory("Integration")]
public class TypedPromptTests
{
    private AiChatClientService theService = new AiChatClientService();
    private AiChatController    theController;

    [TestInitialize]
    public async Task TestInitialise()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        try { await http.GetAsync("http://localhost:11190/aiChat/getOptions"); }
        catch { Assert.Inconclusive("MCP server not reachable on :11190 — start it before running integration tests."); }

        await theService.InitialiseApi();
        theController = new AiChatController(theService, new StubSoccerMatchInfoService());
        await theController.SetModel(new PutParameter { Value = "Claude" });
    }

    [TestMethod]
    public async Task RunTypedPrompt_GoalScorer_ReturnsNonNull()
    {
        var request = new TypedPromptRequest(
            "Who scored the first goal? Use the feed tools to find out.",
            "GoalScorer");

        var actionResult = await theController.RunTypedPrompt(request) as OkObjectResult;

        Assert.IsNotNull(actionResult, "Expected OkObjectResult");

        var json  = actionResult.Value as string;
        Assert.IsFalse(string.IsNullOrWhiteSpace(json), "Response JSON should not be empty");

        var scorer = JsonSerializer.Deserialize<MatchEvent>(json!,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.IsNotNull(scorer, "Response should deserialise to GoalScorer");
        Assert.IsFalse(string.IsNullOrWhiteSpace(scorer.PlayerName),
            "GoalScorer.PlayerName should be non-empty");
    }
}

// =============================================================================
// Stubs — return defaults; enough for controller unit tests (no LLM required).
// =============================================================================

internal class StubTypedAiChatClientService : IAiChatClientService
{
    public Task<T?> RunWorkInProgressPrompt<T>(string prompt) => Task.FromResult(default(T?));
    public Task<T?> RunPromptUnderTest<T>(string prompt)      => Task.FromResult(default(T?));
    public Task<T?> RunTypedPrompt<T>(string prompt)          => Task.FromResult(default(T?));
    public Task<string> RunOriginalPrompt(string prompt)      => Task.FromResult(string.Empty);
    public void SetModelByName(string modelName)              { }
    public void SetSystemPromptByName(string promptName)      { }
}

internal class StubSoccerMatchInfoService : ISoccerMatchInfoService
{
    // Track which method was called and with what prompt so tests can assert on it.
    public string? LastPrompt  { get; private set; }
    public string? LastMethod  { get; private set; }

    public Task<MatchEvent?> FindEventAsync(string prompt)
    {
        (LastMethod, LastPrompt) = (nameof(FindEventAsync), prompt);
        return Task.FromResult<MatchEvent?>(new MatchEvent("Goal", "Test Player", "Home", 1, 10, 0));
    }

    public Task<MatchEventList?> FindEventsAsync(string prompt)
    {
        (LastMethod, LastPrompt) = (nameof(FindEventsAsync), prompt);
        return Task.FromResult<MatchEventList?>(
            new MatchEventList([new MatchEvent("Foul", "Test Player", "Away", 1, 22, 30)]));
    }

    public Task<PlayerAppearance?> FindPlayerAsync(string prompt)
    {
        (LastMethod, LastPrompt) = (nameof(FindPlayerAsync), prompt);
        return Task.FromResult<PlayerAppearance?>(new PlayerAppearance("Test Player", "Home"));
    }

    public Task<PlayerList?> FindPlayersAsync(string prompt)
    {
        (LastMethod, LastPrompt) = (nameof(FindPlayersAsync), prompt);
        return Task.FromResult<PlayerList?>(
            new PlayerList([new PlayerAppearance("Test Player", "Home")]));
    }
}
