using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Common;
using System.Net.Http.Headers;

namespace AutoUpdateTestWorkerApp
{
    public class Informer
    {
        private readonly ILogger<Informer> _logger;
        private readonly WorkerOptions _workerOptions;
        private readonly DeploySettings _deploySettings;

        public Informer(ILogger<Informer> logger, WorkerOptions workerOptions, DeploySettings deploySettings)
        {
            _logger = logger;
            _workerOptions = workerOptions;
            _deploySettings = deploySettings;
        }
        public async void Inform(InformType informType, string version, string message, bool isError, string authenticationBearerToken)
        {
            if (isError)
            {
                _logger.LogError($"{DateTimeOffset.Now}, DeployId {_deploySettings.Id} {version}: {message}");
            }
            else
            {
                _logger.LogInformation($"{DateTimeOffset.Now}, DeployId {_deploySettings.Id} {version}: {message}");
            }

            UriBuilder builder;
            builder = new UriBuilder(new Uri(_workerOptions.ManagerUrl));
            builder.Path = "/api/Update/Inform";

            try
            {
				using (var client = new HttpClient())
				{
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationBearerToken);

                    string jsonString = JsonSerializer.Serialize(new InformItem()
                    {

                        Date = DateTime.UtcNow,
                        InformType = informType,
                        DeployId = _deploySettings.Id,
                        Version = version,
                        Message = message,
                        IsError = isError
                    });
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
					var response = await client.PostAsync(builder.Uri, content);

					if (!response.IsSuccessStatusCode)
					{
                        _logger.LogError($"{DateTimeOffset.Now}, DeployId {_deploySettings.Id} {version}: post of inform data to {builder.ToString()} failed, returning {response.StatusCode.ToString()} {response.ReasonPhrase}");
                    }
                    else
                    {
                        _logger.LogInformation($"{DateTimeOffset.Now}, DeployId {_deploySettings.Id} {version}: post of inform data to {builder.ToString()} succeeded.");
                    }
                }

			}
			catch (Exception ex)
            {
                _logger.LogError($"{DateTimeOffset.Now}, DeployId {_deploySettings.Id} {version}: post of inform data to {builder.ToString()} failed with exception {ex.ToString()}");
            }
        }

    }
}
