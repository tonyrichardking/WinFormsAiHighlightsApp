namespace OllamaMcpWebServer.Controllers
{
    using AiHighlightsMcpServer.Prompt_Engineering;
    using AiHighlightsMcpServer.Services;
    using MCPServer.MCPTools;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using System.Text.Json;
    using static AiChatClientService;

    [ApiController]
    [Route("[controller]")]
    public class AiChatController : ControllerBase
    {
        public class PutParameter
        {
            public string Value { get; set; }
        }

        private readonly IAiChatClientService aiChatService;
        private readonly ISoccerMatchInfoService _matchInfo;

        public AiChatController(IAiChatClientService aiChatService, ISoccerMatchInfoService matchInfo)
        {
            this.aiChatService = aiChatService;
            _matchInfo = matchInfo;
        }

        // GET: aiChat/getSystemPrompts
        [HttpGet("getSystemPrompts", Name = "GetSystemPrompts")]
        public async Task<IActionResult> GetSystemPrompts()
        {
            return Ok(AiChatClientService.AvailableSystemPrompts);
        }

        // GET: aiChat/getModels
        [HttpGet("getModels", Name = "GetModels")]
        public async Task<IActionResult> GetModels()
        {
            return Ok(AiChatClientService.AvailableModels);
        }

        // GET: aiChat/getTools
        [HttpGet("getTools", Name = "GetTools")]
        public async Task<IActionResult> GetTools()
        {
            return Ok(AiChatClientService.AvailableTools.Select(t => t.Name).ToList());
        }

        // GET: aiChat/getOptions
        [HttpGet("getOptions", Name = "GetOptions")]
        public async Task<IActionResult> GetOptions()
        {
            return Ok(AiChatClientService.GetOptions());
        }

        // GET: aiChat/getResultTypes
        [HttpGet("getResultTypes", Name = "GetResultTypes")]
        public IActionResult GetResultTypes()
        {
            return Ok(SoccerResultTypeCatalog.Descriptions);
        }

        // GET: aiChat/runPrompt?prompt=hello
        [HttpGet("runPrompt", Name = "RunPrompt")]
        public async Task<IActionResult> RunPrompt([FromQuery] string prompt)
        {
            string result = await aiChatService.RunOriginalPrompt(prompt);
            return Ok(result);
        }

        // PUT: aiChat/setModel
        [HttpPut("setModel", Name = "SetModel")]
        public async Task<IActionResult> SetModel([FromBody] PutParameter param)
        {
            aiChatService.SetModelByName(param.Value);
            return Ok();
        }

        // PUT: aiChat/setSystemPrompt
        [HttpPut("setSystemPrompt", Name = "SetSystemPrompt")]
        public async Task<IActionResult> SetSystemPrompt([FromBody] PutParameter param)
        {
            aiChatService.SetSystemPromptByName(param.Value);
            return Ok();
        }

        // POST: aiChat/runTypedPrompt
        // Body: { "prompt": "...", "resultType": "Auto" }   // or a concrete type to override
        [EnableCors]
        [HttpPost("runTypedPrompt", Name = "RunTypedPrompt")]
        public async Task<IActionResult> RunTypedPrompt([FromBody] TypedPromptRequest request)
        {
            // "Auto" (or empty) = let the model choose the shape. Otherwise it's an override,
            // which must name a type the catalog knows.
            bool isAuto = string.IsNullOrWhiteSpace(request.ResultType)
                          || request.ResultType.Equals("Auto", StringComparison.OrdinalIgnoreCase);

            if (!isAuto && !SoccerResultTypeCatalog.Descriptions.ContainsKey(request.ResultType))
            {
                return BadRequest(new
                {
                    error = $"Unknown resultType '{request.ResultType}'.",
                    validTypes = SoccerResultTypeCatalog.Descriptions.Keys
                });
            }

            // One path for both modes. The override (when not Auto) narrows the tool set;
            // either way we get back the same AutoResult envelope.
            AutoResult? result = isAuto
                ? await _matchInfo.FindResultsAsync(request.Prompt)
                : await _matchInfo.FindResultsAsync(request.Prompt, request.ResultType);

            if (result is null)
                return Ok(JsonSerializer.Serialize(new AutoResult("Text", "No matching results found.")));

            return Ok(JsonSerializer.Serialize(result));
        }
    }
}

