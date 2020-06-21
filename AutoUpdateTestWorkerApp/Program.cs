using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// based on https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-3.1&tabs=visual-studio
namespace AutoUpdateTestWorkerApp
{
    public class Program
    {
        private const string AUTOUPDATER_PROCESS_EXEC = "Updater.Process";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService() //https://csharp.christiannagel.com/2019/10/15/windowsservice/
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile($"{AUTOUPDATER_PROCESS_EXEC}.json");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
 
                    WorkerOptions workerOptions = configuration.GetSection("WorkerOptions").Get<WorkerOptions>();
                    services.AddSingleton(workerOptions);

                    DeploySettings deploySettings = configuration.GetSection("DeploySettings").Get<DeploySettings>();
                    services.AddSingleton(deploySettings);

                    services.AddSingleton<Informer>();

                    services.AddHostedService<Worker>();
                });
    }
}
