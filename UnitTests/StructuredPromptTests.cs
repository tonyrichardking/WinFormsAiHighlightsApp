using Microsoft.AspNetCore.Mvc;
using OllamaMcpWebServer.Controllers;
using System.Text.Json;

namespace UnitTests;

// =============================================================================
// DTO serialisation tests
// These verify the StructuredPromptRequest record and its children deserialise
// correctly from JSON — no server, no LLM, no network required.
// =============================================================================

[TestClass]
public class StructuredPromptDtoTests
{
    [TestMethod]
    public void ExampleJson_DeserializesWithoutError()
    {
        var result = JsonSerializer.Deserialize<StructuredPromptRequest>(ExampleStructuredRequestJson.json);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ExampleJson_UserInput_IsPreserved()
    {
        var request = Deserialize();
        Assert.AreEqual(
            "Hello, please introduce yourself and list your capabilities.",
            request.StructuredPrompt.UserPrompt.UserInput);
    }

    [TestMethod]
    public void ExampleJson_Metadata_Guid_IsPreserved()
    {
        var request = Deserialize();
        Assert.AreEqual(
            new Guid("035E5253-E7B2-41A8-81D3-7BA70AC889AA"),
            request.StructuredPrompt.Metadata.MessageGuid);
    }

    [TestMethod]
    public void ExampleJson_Metadata_Sequence_IsPreserved()
    {
        var request = Deserialize();
        Assert.AreEqual(1, request.StructuredPrompt.Metadata.Sequence);
    }

    [TestMethod]
    public void ExampleJson_ResponseFormats_AreNonEmpty()
    {
        var formats = Deserialize().StructuredPrompt.SystemPrompt.ResponseFormat.Required;
        Assert.IsTrue(formats.Count > 0, "Expected at least one response format");
    }

    [TestMethod]
    public void ExampleJson_FfmpegSubclipping_FormatIsPresent()
    {
        var formats = Deserialize().StructuredPrompt.SystemPrompt.ResponseFormat.Required;
        Assert.IsTrue(formats.Any(f => f.Name == "ffMpegSubclipping"),
            "ffMpegSubclipping format should be present");
    }

    [TestMethod]
    public void ExampleJson_CuratorSubclipping_FormatIsPresent()
    {
        var formats = Deserialize().StructuredPrompt.SystemPrompt.ResponseFormat.Required;
        Assert.IsTrue(formats.Any(f => f.Name == "curatorSubclipping"),
            "curatorSubclipping format should be present");
    }

    [TestMethod]
    public void ExampleJson_EachFormatItem_HasAllRequiredFields()
    {
        var formats = Deserialize().StructuredPrompt.SystemPrompt.ResponseFormat.Required;
        foreach (var item in formats)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(item.Name),    $"Format item missing Name");
            Assert.IsFalse(string.IsNullOrWhiteSpace(item.Format),  $"Format item '{item.Name}' missing Format");
            Assert.IsFalse(string.IsNullOrWhiteSpace(item.Purpose), $"Format item '{item.Name}' missing Purpose");
        }
    }

    [TestMethod]
    public void RoundTrip_SerializeDeserialize_PreservesUserInput()
    {
        var original = Deserialize();
        var json     = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<StructuredPromptRequest>(json)!;

        Assert.AreEqual(
            original.StructuredPrompt.UserPrompt.UserInput,
            restored.StructuredPrompt.UserPrompt.UserInput);
    }

    [TestMethod]
    public void RoundTrip_SerializeDeserialize_PreservesFormatCount()
    {
        var original = Deserialize();
        var json     = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<StructuredPromptRequest>(json)!;

        Assert.AreEqual(
            original.StructuredPrompt.SystemPrompt.ResponseFormat.Required.Count,
            restored.StructuredPrompt.SystemPrompt.ResponseFormat.Required.Count);
    }

    [TestMethod]
    public void EmptyJson_ThrowsOnDeserialize()
    {
        Assert.ThrowsExactly<JsonException>(() =>
            JsonSerializer.Deserialize<StructuredPromptRequest>("{}"));
    }

    private static StructuredPromptRequest Deserialize() =>
        JsonSerializer.Deserialize<StructuredPromptRequest>(ExampleStructuredRequestJson.json)
        ?? throw new InvalidOperationException("Deserialize returned null");
}

// =============================================================================
// Controller unit tests
// These test the controller's routing and response-wrapping logic in isolation,
// using a hand-rolled stub for IAiChatClientService — no LLM or server needed.
// =============================================================================

[TestClass]
public class AiChatControllerTests
{
    // -------------------------------------------------------------------------
    // RunStructuredPrompt
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task RunStructuredPrompt_ReturnsOk()
    {
        var (controller, _) = MakeController("ai response");
        var request = MakeRequest("test prompt");

        var result = await controller.RunStructuredPrompt(request);

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public async Task RunStructuredPrompt_ReturnsServiceResponse()
    {
        var (controller, _) = MakeController("the model said this");
        var result = await controller.RunStructuredPrompt(MakeRequest("anything")) as OkObjectResult;

        Assert.AreEqual("the model said this", result!.Value);
    }

    [TestMethod]
    public async Task RunStructuredPrompt_PassesRequestToService()
    {
        var (controller, stub) = MakeController("ok");
        var request = MakeRequest("show me the goals");

        await controller.RunStructuredPrompt(request);

        Assert.AreSame(request, stub.LastStructuredRequest,
            "Controller should pass the request object through to the service unchanged");
    }

    // -------------------------------------------------------------------------
    // RunPrompt
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task RunPrompt_ReturnsOk()
    {
        var (controller, _) = MakeController("response");
        var result = await controller.RunPrompt("hello");

        Assert.IsInstanceOfType<OkObjectResult>(result);
    }

    [TestMethod]
    public async Task RunPrompt_ReturnsServiceResponse()
    {
        var (controller, _) = MakeController("hello back");
        var result = await controller.RunPrompt("hello") as OkObjectResult;

        Assert.AreEqual("hello back", result!.Value);
    }

    [TestMethod]
    public async Task RunPrompt_PassesPromptStringToService()
    {
        var (controller, stub) = MakeController("ok");
        await controller.RunPrompt("find all the fouls");

        Assert.AreEqual("find all the fouls", stub.LastPrompt);
    }

    // -------------------------------------------------------------------------
    // SetModel
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task SetModel_ReturnsOk()
    {
        var (controller, _) = MakeController();
        var result = await controller.SetModel(new AiChatController.PutParameter { Value = "llama3.2:latest" });

        Assert.IsInstanceOfType<OkResult>(result);
    }

    [TestMethod]
    public async Task SetModel_PassesModelNameToService()
    {
        var (controller, stub) = MakeController();
        await controller.SetModel(new AiChatController.PutParameter { Value = "deepseek-r1:latest" });

        Assert.AreEqual("deepseek-r1:latest", stub.LastSetModelName);
    }

    // -------------------------------------------------------------------------
    // SetSystemPrompt
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task SetSystemPrompt_ReturnsOk()
    {
        var (controller, _) = MakeController();
        var result = await controller.SetSystemPrompt(new AiChatController.PutParameter { Value = "Sports" });

        Assert.IsInstanceOfType<OkResult>(result);
    }

    [TestMethod]
    public async Task SetSystemPrompt_PassesPromptNameToService()
    {
        var (controller, stub) = MakeController();
        await controller.SetSystemPrompt(new AiChatController.PutParameter { Value = "Helpful" });

        Assert.AreEqual("Helpful", stub.LastSetSystemPromptName);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static (AiChatController controller, StubAiChatClientService stub) MakeController(string serviceResponse = "")
    {
        var stub = new StubAiChatClientService(serviceResponse);
        return (new AiChatController(stub), stub);
    }

    private static StructuredPromptRequest MakeRequest(string userInput) =>
        new()
        {
            StructuredPrompt = new StructuredPrompt
            {
                Metadata = new Metadata
                {
                    DateTime    = DateTimeOffset.UtcNow,
                    MessageGuid = Guid.NewGuid(),
                    Sequence    = 1,
                },
                UserPrompt = new UserPrompt { UserInput = userInput },
                SystemPrompt = new SystemPrompt
                {
                    ResponseFormat = new ResponseFormat
                    {
                        Required = new List<ResponseFormatItem>
                        {
                            new() { Name = "expositoryText", Format = "Plain text.", Purpose = "Clarity." }
                        }
                    }
                }
            }
        };
}

// =============================================================================
// Stub — hand-rolled test double for IAiChatClientService.
// Captures arguments so tests can assert on what was passed.
// =============================================================================

internal class StubAiChatClientService : IAiChatClientService
{
    private readonly string _response;

    public string?                  LastPrompt             { get; private set; }
    public StructuredPromptRequest? LastStructuredRequest  { get; private set; }
    public string?                  LastSetModelName       { get; private set; }
    public string?                  LastSetSystemPromptName { get; private set; }

    public StubAiChatClientService(string response = "") => _response = response;

    public Task<string> RunPrompt(string prompt)
    {
        LastPrompt = prompt;
        return Task.FromResult(_response);
    }

    public Task<string> RunStructuredPrompt(StructuredPromptRequest request)
    {
        LastStructuredRequest = request;
        return Task.FromResult(_response);
    }

    public void SetModelByName(string modelName)       => LastSetModelName = modelName;
    public void SetSystemPromptByName(string promptName) => LastSetSystemPromptName = promptName;
}
