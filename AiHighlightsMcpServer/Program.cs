using Microsoft.Extensions.Logging.EventLog;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestServices;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //  at base address {Utils.TheMdcSparqlServiceEndpointAddress}");
            Log.Information($"\n\n=============================================================================\nMetadata Central Windows Service starting up.\n=============================================================================\n");

            // https://stackoverflow.com/questions/70571849/host-asp-net-6-in-a-windows-service
            WebApplicationOptions webApplicationOptions = new()
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = args,
                ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
            };

            var builder = WebApplication.CreateBuilder(webApplicationOptions);

            // Make the ASP.NET Core host listen on a known HTTP URL for the MCP HTTP transport.
            // Change the URL/port as required (e.g. "http://0.0.0.0:5252" for all interfaces).
            builder.WebHost.UseUrls("http://localhost:5252");

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //
            // add custom services
            //

            // asynchronous database configuration 
            // IHostedService is specifically designed for background tasks that run independently of user
            // interactions. It allows applications to perform operations like data processing, scheduled jobs,
            // or handling system events in the background.
            //builder.Services.AddHostedService<SystemConfigurationService>();

            // The Singleton design pattern ensures that a class has only one instance throughout the application's
            // lifetime. This is useful for shared resources like configuration settings or logging services
            builder.Services.AddSingleton<Ma3FeedDataProviderService>();
            builder.Services.AddSingleton<AiChatClientService>();

            // If you have an MCP client hosted service you want to run in the same process, enable it.
            // builder.Services.AddHostedService<TestMcpClientHostedService>();

            //
            // add framework services
            //

            builder.Host.UseWindowsService();
            builder.Services.AddControllers();
            builder.Host.UseSerilog();

            builder.Services.AddSwaggerGenNewtonsoftSupport();

            if (OperatingSystem.IsWindows())
            {
                builder.Services.Configure<EventLogSettings>(config =>
                {
                    if (OperatingSystem.IsWindows())
                    {
                        config.LogName = "Sample Service";
                        config.SourceName = "Sample Service Source";
                    }
                });

                // Register the Swagger generator, defining one or more Swagger documents
                builder.Services.AddSwaggerGen(c =>
                {
                    // The c.SwaggerDoc and c.SwaggerEndpoint names must agree.  Nobody tells you this!
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MdcApi", Version = "v1" });

                    // https://stackoverflow.com/questions/46071513/swagger-error-conflicting-schemaids-duplicate-schemaids-detected-for-types-a-a#47306578
                    c.SchemaGeneratorOptions = new SchemaGeneratorOptions { SchemaIdSelector = type => type.FullName };
                });
            }

            // 
            // Model Context Protocol (MCP) configuration
            // https://csharp.sdk.modelcontextprotocol.io

            // Add the Model Context Protocol (MCP) server to the ASP.NET Core application.
            builder.Services
                .AddMcpServer()
                .WithHttpTransport()
                .WithHttpTransport(options =>
                {
                    // https://csharp.sdk.modelcontextprotocol.io/concepts/stateless/stateless.html
                    options.Stateless = true;
                })
                //.WithHttpTransport(options =>
                //{
                //    // Common options — adjust names to match your ModelContextProtocol package's option model:
                //    // - BasePath or PathBase: the request path segment the MCP endpoint listens on
                //    // - ListenUrls/Address: optional; we configured Kestrel above via UseUrls
                //    options.PathBase = "/mcp"; // requests will be POSTed to http://localhost:5252/mcp (change as needed)
                //    // options.ListenUrls = new[] { "http://localhost:5252" }; // optional if you prefer to set transport-level URLs
                //})
                .WithToolsFromAssembly();

            var app = builder.Build();

            // MapMcp is an extension method exposed in ModelContextProtocol.AspNetCore to configure MCP server endpoints.
            // (/sse and /messages endpoints are no longer supported in the latest version of the ModelContextProtocol package).

            app.MapMcp("/mcp");

            // 
            // middleware configuration
            //

            app.UseStaticFiles();

            // Configure the HTTP request pipeline.

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                // The c.SwaggerDoc and c.SwaggerEndpoint names must agree.  Nobody tells you this!
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MdcApi V1");

                // Serve the Swagger UI at the app's root (http://localhost:<port>/)
                c.RoutePrefix = string.Empty;
            });

            // Redirect HTTP Requests to HTTPS (not implemented yet)
            //app.UseHttpsRedirection();

            // A call to UseRouting must be followed by a call to UseEndpoints
            app.UseRouting();

            // TODO:- 
            // https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0
            // Must follow a call to UseRouting()
            // Allows CORS requests from all origins with any scheme (http or https).
            // AllowAnyOrigin is insecure because any website can make cross-origin requests to the app.
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            // When authorizing a resource that is routed using endpoint routing, this call must appear between
            // the calls to app.UseRouting() and app.UseEndpoints(...) for the middleware to function correctly.
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}
