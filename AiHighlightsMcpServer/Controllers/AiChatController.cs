namespace OllamaMcpWebServer.Controllers
{
    using AiHighlightsMcpServer.Prompt_Engineering;
    using MCPServer.MCPTools;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using ModelContextProtocol.Protocol;
    using ModelContextProtocol.Server;
    using System.Net.NetworkInformation;
    using TestServices;

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
            Dictionary<string, string> result = AiChatClientService.AvailableSystemPrompts;

            return Ok(result);
        }

        // GET: aiChat/getModels
        [HttpGet("getModels", Name = "GetModels")]
        public async Task<IActionResult> GetModels()
        {
            List<string> result = AiChatClientService.AvailableModels;

            return Ok(result);
        }

        // GET: aiChat/getTools
        [HttpGet("getTools", Name = "GetTools")]
        public async Task<IActionResult> GetTools()
        {
            List<string> result = AiChatClientService.AvailableTools.Select(t => t.Name).ToList();

            return Ok(result);
        }

        // GET: aiChat/getOptions
        [HttpGet("getOptions", Name = "GetOptions")]
        public async Task<IActionResult> GetOptions()
        {
            string result = AiChatClientService.GetOptions();

            return Ok(result);
        }

        // The shape you want back. With structured output, this record IS the spec —
        // the framework derives a JSON schema from it and deserialises into it, so
        // there's no hand-authored JSON and no casing footgun: the type is the contract.
        public record GoalScorer(string PlayerName, string Team, int Period, int TimeMin, int TimeSec);

        public record GoalsResult(GoalScorer[] Goals);

        // GET: aiChat/runPrompt?prompt=hello
        [HttpGet("runPrompt", Name = "RunPrompt")]
        public async Task<IActionResult> RunPrompt([FromQuery] string prompt)
        {
            string result = await aiChatService.RunPromptUnderTest(prompt);
            //string result = await aiChatService.RunDebugPrompt(prompt);

            return Ok(result);
        }

        // PUT: aiChat/setModel?model=MODEL_NAME
        [HttpPut("setModel", Name = "SetModel")]
        public async Task<IActionResult> SetModel([FromBody] PutParameter param)
        {
            aiChatService.SetModelByName(param.Value);
            return Ok();
        }

        // PUT: aiChat/setSystemPrompt?prompt=PROMPT_NAME
        [HttpPut("setSystemPrompt", Name = "SetSystemPrompt")]
        public async Task<IActionResult> SetSystemPrompt([FromBody] PutParameter param)
        {
            aiChatService.SetSystemPromptByName(param.Value);
            return Ok();
        }

        // POST: MdcApi/runQuery
        [EnableCors]
        [HttpPost("runStructuredPrompt", Name = "RunStructuredPrompt")]
        public async Task<IActionResult> RunStructuredPrompt([FromBody] string prompt)
        {
            string result = await aiChatService.RunPromptUnderTest(prompt);

            //return new RunQueryPlayersResponseDto.Root { errorMessage = ConfigurationNotFinishedErrorMessage };

            return Ok(result);
        }
    }
}

