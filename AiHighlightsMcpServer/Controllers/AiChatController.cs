namespace OllamaMcpWebServer.Controllers
{
    using AiHighlightsMcpServer.Prompt_Engineering;
    using AiHighlightsMcpServer.Services;
    using MCPServer.MCPTools;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using System.Text.Json;

    [ApiController]
    [Route("[controller]")]
    public class AiChatController : ControllerBase
    {
        public class PutParameter
        {
            public string Value { get; set; }
        }

        private readonly IAiChatClientService aiChatService;

        public AiChatController(IAiChatClientService aiChatService)
        {
            this.aiChatService = aiChatService;
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
        // Body: { "prompt": "...", "resultType": "GoalScorer" }
        [EnableCors]
        [HttpPost("runTypedPrompt", Name = "RunTypedPrompt")]
        public async Task<IActionResult> RunTypedPrompt([FromBody] TypedPromptRequest request)
        {
            object? result = request.ResultType switch
            {
                "MatchEvent"           => await aiChatService.RunPromptUnderTest<MatchEvent>(request.Prompt),
                "MatchEventList"       => await aiChatService.RunPromptUnderTest<MatchEventList>(request.Prompt),
                "PlayerAppearance"     => await aiChatService.RunPromptUnderTest<PlayerAppearance>(request.Prompt),
                "PlayerList"           => await aiChatService.RunPromptUnderTest<PlayerList>(request.Prompt),
                _ => null
            };

            if (result is null && !SoccerResultTypeCatalog.Descriptions.ContainsKey(request.ResultType))
            {
                return BadRequest(new
                {
                    error   = $"Unknown resultType '{request.ResultType}'.",
                    validTypes = SoccerResultTypeCatalog.Descriptions.Keys
                });
            }

            return Ok(JsonSerializer.Serialize(result));
        }
    }
}

