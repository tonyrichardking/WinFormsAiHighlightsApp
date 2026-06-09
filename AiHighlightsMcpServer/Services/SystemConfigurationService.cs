// https://weblogs.asp.net/sreejukg/running-background-tasks-in-asp-net-applications

//using DataServices;
using Microsoft.Extensions.Hosting;
using Serilog;
//using WindowsService.Controllers;

namespace WindowsService.Services
{
    public class SystemConfigurationService : BackgroundService
    {
        private static readonly string myId = "MDC Startup";
        //private readonly DataLookupService theDataLookupService;

        //public SystemConfigurationService(DataLookupService dataLookupService)
        //{
        //    theDataLookupService = dataLookupService;
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information($"\n\n=============================================================================\nStarting system configuration.  Service will be unavailable until completion.\n=============================================================================\n");

            await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);    // allow the Main() thread to finish
            //bool configuredOk = new StartupUtilities().EnsureConfigurationInDatabase(theDataLookupService, useNew: true);
            //MdcApiController.SetConfigurationFinished();
            //EtlWorkerService.SetConfigurationFinished();

            Log.Information($"\n\n=========================================================\nFinished system configuration.  Service is now available.\n=========================================================\n");
        }
    }
}