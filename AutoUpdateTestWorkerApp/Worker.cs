using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Updater;
using Common;
//using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace AutoUpdateTestWorkerApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerOptions _options;
        private readonly Informer _informer;
        private readonly int _workerIntervalMillis = 3600000;
        private readonly string _signingKey = "1c281667-a1da-46ad-a895-b89aff3f5b4e";

        public Worker(ILogger<Worker> logger, WorkerOptions options, Informer informer)
        {
            _logger = logger;
            _options = options;
            _informer = informer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            AutoUpdateImpl autoUpdateImpl = new AutoUpdateImpl(_options.WinUrl, _options.MacUrl, _options.PackageUrl, _options.ServiceName);
            _logger.LogInformation($"autoUpdateImpl: {JsonSerializer.Serialize(autoUpdateImpl)}");
            AutoUpdater autoUpdater = new AutoUpdater(autoUpdateImpl);

            string runningVersionString = Assembly.GetEntryAssembly().GetName().Version.ToString();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Utilities.IsTimeBetween(DateTime.Now, new TimeSpan(_options.EndOfDayLocalHour, 0, 0), new TimeSpan(_options.BeginningOfDayLocalHour, 0, 0)))
                {
                    try
                    {

   
                        Task<bool> isUpdateAvailable = autoUpdater.CheckUpdateAvailableAsync(getAuthenticationBearerToken());
                        if (isUpdateAvailable.Result)
                        {
                            string dir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                            Task task = Task.Run(async () => { await autoUpdater.UpdateAsync(dir, getAuthenticationBearerToken()); });
                            _logger.LogInformation($"dir: {dir}");
                            _informer.Inform(InformType.VersionUpdate, runningVersionString, "Update check found a new version. Installing.", false, getAuthenticationBearerToken());
                        }
                        else
                        {
                            _informer.Inform(InformType.VersionUpdate, runningVersionString, "Update check did not find a new version.", false, getAuthenticationBearerToken());
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionString = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                        _informer.Inform(InformType.VersionUpdate, runningVersionString, $"Upate check encountered and error. Skipping this update and retrying in {_workerIntervalMillis / 60000} minutes. Exception reported: {exceptionString}", true, getAuthenticationBearerToken());
                    }
                }

                await Task.Delay(_workerIntervalMillis, stoppingToken); //1 hr
            }
        }

        public string getAuthenticationBearerToken()
        {
            return Utilities.generateJWTToken(_signingKey);
        }
    }
}
