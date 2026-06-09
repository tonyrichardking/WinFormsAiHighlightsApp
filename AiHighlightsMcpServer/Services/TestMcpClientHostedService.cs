// https://deepwiki.com/modelcontextprotocol/csharp-sdk/3.1-client-factory

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace TestServices
{
    /*
        public class TestMcpClientHostedService : IHostedService
        {
            private readonly ILogger<TestMcpClientHostedService> _logger;
            private McpClient? _client;

            public TestMcpClientHostedService(ILogger<TestMcpClientHostedService> logger)
            {
                _logger = logger;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Starting MCP client...");

                // HTTP transport connects to an existing server rather than launching a new process.

                McpClient _client = await McpClient.CreateAsync
                (
                //new HttpClientTransport(new HttpClientTransportOptions())
                );

                _logger.LogInformation("MCP client connected.");
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                if (_client != null)
                {
                    await _client.DisposeAsync();
                    _logger.LogInformation("MCP client stopped.");
                }
            }
        }
    */
}
