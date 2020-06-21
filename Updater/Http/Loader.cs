using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater

{
    public class Loader
    {
        internal static async Task<T> LoadAsync<T>(CancellationToken cancellationToken, AuthenticationHeaderValue authHeader, HttpMethod method, Dictionary<string, string> parameters, string url)
        {
            Stream stream = await LoadStreamAsync(cancellationToken, authHeader, method, parameters, url);
            return DeserializeJsonFromStream<T>(stream);
        }

        internal static async Task<Stream> LoadStreamAsync(CancellationToken cancellationToken, AuthenticationHeaderValue authHeader, HttpMethod method, object parameters, string url)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                //specify to use TLS 1.2 as default connection
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                client.DefaultRequestHeaders.Authorization = authHeader;
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                {
                    if (method == HttpMethod.Post)
                    {
                        string jsonString = JsonSerializer.Serialize(parameters);
                        request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json"); //new FormUrlEncodedContent(parameters);
                    }

                    using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        Stream stream = await response.Content.ReadAsStreamAsync();
                        
                        if (response.IsSuccessStatusCode)
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            byte[] buffer = new byte[16 * 1024];
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, read);
                            }
                            return memoryStream;
                        }
                        string content = await StreamToStringAsync(stream);
                        throw new ApiException
                        {
                            Url = url,
                            Method = method.Method,
                            StatusCode = (int)response.StatusCode,
                            Content = content
                        };
                    }
                }
            }
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default(T);
            }
            stream.Position = 0;
            using (StreamReader sr = new StreamReader(stream))
            {
                using (Newtonsoft.Json.JsonTextReader jtr = new Newtonsoft.Json.JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer js = new Newtonsoft.Json.JsonSerializer();
                    T result = js.Deserialize<T>(jtr);
                    return result;
                }
            }
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    content = await sr.ReadToEndAsync();
                }
            }

            return content;
        }
    }
}
