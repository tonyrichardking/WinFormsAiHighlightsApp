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
        // Body: { "prompt": "...", "resultType": "MatchEventList" }
        [EnableCors]
        [HttpPost("runTypedPrompt", Name = "RunTypedPrompt")]
        public async Task<IActionResult> RunTypedPrompt([FromBody] TypedPromptRequest request)
        {
            if (!SoccerResultTypeCatalog.Descriptions.ContainsKey(request.ResultType))
            {
                return BadRequest(new
                {
                    error      = $"Unknown resultType '{request.ResultType}'.",
                    validTypes = SoccerResultTypeCatalog.Descriptions.Keys
                });
            }

            object? result = request.ResultType switch
            {
                "MatchEvent"       => await _matchInfo.FindEventAsync(request.Prompt),
                "MatchEventList"   => await _matchInfo.FindEventsAsync(request.Prompt),
                "PlayerAppearance" => await _matchInfo.FindPlayerAsync(request.Prompt),
                "PlayerList"       => await _matchInfo.FindPlayersAsync(request.Prompt),
                _                  => null
            };

            return Ok(JsonSerializer.Serialize(result));
        }
    }
}

