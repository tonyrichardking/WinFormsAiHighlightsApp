namespace OllamaMcpWebServer.Controllers
{
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

        // GET: aiChat/runPrompt?prompt=hello
        [HttpGet("runPrompt", Name = "RunPrompt")]
        public async Task<IActionResult> RunPrompt([FromQuery] string prompt)
        {
            string result = await aiChatService.RunPrompt(prompt);

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
        public async Task<IActionResult> RunStructuredPrompt([FromBody] StructuredPromptRequest structuredPromptRequest)
        {
            string result = await aiChatService.RunStructuredPrompt(structuredPromptRequest);

            //return new RunQueryPlayersResponseDto.Root { errorMessage = ConfigurationNotFinishedErrorMessage };

            return Ok(result);
        }
    }
}

