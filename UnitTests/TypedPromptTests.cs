using AiHighlightsMcpServer.Prompt_Engineering;
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
            "GoalScorer", "GoalList",
            "MatchEvent", "MatchEventList",
            "HighlightSegment", "HighlightSegmentList",
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
        var controller = MakeController();
        var result = controller.GetResultTypes();

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public void GetResultTypes_ReturnsDictionary_WithAllTypes()
    {
        var controller = MakeController();
        var ok   = (OkObjectResult)controller.GetResultTypes();
        var dict = ok.Value as Dictionary<string, string>;

        Assert.IsNotNull(dict);
        Assert.IsTrue(dict.ContainsKey("GoalScorer"));
        Assert.IsTrue(dict.ContainsKey("HighlightSegmentList"));
    }

    // -------------------------------------------------------------------------
    // runTypedPrompt — known types return Ok
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task RunTypedPrompt_GoalScorer_ReturnsOk()
        => await AssertKnownTypeReturnsOk("GoalScorer");

    [TestMethod]
    public async Task RunTypedPrompt_GoalList_ReturnsOk()
        => await AssertKnownTypeReturnsOk("GoalList");

    [TestMethod]
    public async Task RunTypedPrompt_MatchEvent_ReturnsOk()
        => await AssertKnownTypeReturnsOk("MatchEvent");

    [TestMethod]
    public async Task RunTypedPrompt_MatchEventList_ReturnsOk()
        => await AssertKnownTypeReturnsOk("MatchEventList");

    [TestMethod]
    public async Task RunTypedPrompt_HighlightSegment_ReturnsOk()
        => await AssertKnownTypeReturnsOk("HighlightSegment");

    [TestMethod]
    public async Task RunTypedPrompt_HighlightSegmentList_ReturnsOk()
        => await AssertKnownTypeReturnsOk("HighlightSegmentList");

    [TestMethod]
    public async Task RunTypedPrompt_PlayerAppearance_ReturnsOk()
        => await AssertKnownTypeReturnsOk("PlayerAppearance");

    [TestMethod]
    public async Task RunTypedPrompt_PlayerList_ReturnsOk()
        => await AssertKnownTypeReturnsOk("PlayerList");

    // -------------------------------------------------------------------------
    // runTypedPrompt — unknown type returns BadRequest
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task RunTypedPrompt_UnknownType_ReturnsBadRequest()
    {
        var controller = MakeController();
        var request    = new TypedPromptRequest("any prompt", "UnknownType");

        var result = await controller.RunTypedPrompt(request);

        Assert.IsInstanceOfType<BadRequestObjectResult>(result,
            "An unknown resultType should return 400 Bad Request");
    }

    [TestMethod]
    public async Task RunTypedPrompt_UnknownType_ResponseContainsValidTypes()
    {
        var controller = MakeController();
        var result     = await controller.RunTypedPrompt(new TypedPromptRequest("p", "BadType"))
                         as BadRequestObjectResult;

        var json = JsonSerializer.Serialize(result!.Value);
        Assert.IsTrue(json.Contains("validTypes"), "Bad-request body should list validTypes");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static AiChatController MakeController()
        => new AiChatController(new StubTypedAiChatClientService());

    private static async Task AssertKnownTypeReturnsOk(string resultType)
    {
        var controller = MakeController();
        var request    = new TypedPromptRequest("test prompt", resultType);

        var result = await controller.RunTypedPrompt(request);

        Assert.IsInstanceOfType<OkObjectResult>(result,
            $"ResultType '{resultType}' should return 200 OK");
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
        theController = new AiChatController(theService);
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
// Stub — returns default(T) for all typed calls; enough for controller tests.
// =============================================================================

internal class StubTypedAiChatClientService : IAiChatClientService
{
    public string? LastPrompt      { get; private set; }
    public string? LastResultType  { get; private set; }

    public Task<T?> RunPromptUnderTest<T>(string prompt)
    {
        LastPrompt = prompt;
        return Task.FromResult(default(T?));
    }

    public Task<T?> RunTypedPrompt<T>(string prompt)
        => Task.FromResult(default(T?));

    public Task<string> RunOriginalPrompt(string prompt)
        => Task.FromResult(string.Empty);

    public void SetModelByName(string modelName)       { }
    public void SetSystemPromptByName(string promptName) { }
}
