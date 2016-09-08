using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebLoader.Logging;

namespace WebLoader.Http
{
    public interface IHttpGateway
    {
        Task<PushResponse> GetDelayedAsync(string requestUrl, string verb, string content, int staggeredDelay,
            int requestGrouping,
            CancellationToken token);

        Task<IEnumerable<PushResponse>> MakeStaggeredRequests(IRunSpecification runSpec, int requestCount,
            int requestGrouping, CancellationToken token);
    }

    public class HttpGateway : IHttpGateway
    {
        private static readonly Random random = new Random();
        private readonly HttpClient _httpClient;
        private readonly IPusherLogger _logger;

        public HttpGateway(IPusherLogger logger, string baseUrl, int timeoutMs)
            : this(logger, baseUrl, timeoutMs, new Dictionary<string, string>())
        {
        }

        public HttpGateway(IPusherLogger logger, string baseUrl, int timeoutMs,
            IDictionary<string, string> defaultHeaders)
        {
            //ServicePointManager.UseNagleAlgorithm = false;
            //ServicePointManager.DefaultConnectionLimit = 100;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // Used in test scenarios
            //ServicePointManager.ReusePort = true; // .NET 4.6 feature to allow more than 65k req/sec (available ports)


            _logger = logger;

            // Flesh out defaults for the underlying HttpClient
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMilliseconds(timeoutMs)
            };

            foreach (var kv in defaultHeaders)
            {
                if (kv.Key == "Content-Type")
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(kv.Value));
                else
                {
                    _httpClient.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                }
            }
        }

        public async Task<IEnumerable<PushResponse>> MakeStaggeredRequests(
            IRunSpecification runSpec,
            int requestCount,
            int requestGrouping,
            CancellationToken token)
        {
            // Fire off request tasks with staggered delay of 1-999 milliseconds
            var requestTasks = Enumerable.Range(1, requestCount)
                .Select(i => GetDelayedAsync(
                    runSpec.GenerateRelativeUrl(), runSpec.Verb, runSpec.Content, random.Next(1, 1000), requestGrouping,
                    token))
                .ToList();

            return await Task.WhenAll(requestTasks)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<PushResponse> GetDelayedAsync(string relativeRequestUrl, string verb, string content,
            int staggeredDelay,
            int requestGrouping, CancellationToken token)
        {
            await Task.Delay(staggeredDelay, token).ConfigureAwait(false);

            var pushResponse = new PushResponse
            {
                RequestGrouping = requestGrouping,
                RequestTime = DateTime.UtcNow
            };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                //var response = await _httpClient.GetAsync(relativeRequestUrl, token)
                //    .ConfigureAwait(continueOnCapturedContext: false);
                HttpResponseMessage response = null;

                switch (verb)
                {
                    case "POST":
                    {
                        var Content = new StringContent(content, Encoding.UTF8,
                            _httpClient.DefaultRequestHeaders.Accept.ToString());
                        response = await _httpClient.PostAsync(_httpClient.BaseAddress, Content, token)
                            .ConfigureAwait(false);
                        break;
                    }
                    case "GET":
                        response = await _httpClient.GetAsync(relativeRequestUrl, token)
                            .ConfigureAwait(false);
                        break;
                    case "PUT":
                    {
                        var Content = new StringContent(content, Encoding.UTF8, "application/json");
                        response = await _httpClient.PutAsync(_httpClient.BaseAddress, Content, token)
                            .ConfigureAwait(false);
                        break;
                    }
                    default: //Verb GET
                        response = await _httpClient.GetAsync(relativeRequestUrl, token)
                            .ConfigureAwait(false);
                        break;
                }


                stopwatch.Stop();

                pushResponse = PopulatePushResponse(pushResponse, response, stopwatch);
            }
            catch (TimeoutException)
            {
                pushResponse.IsSuccessful = false;
                pushResponse.Body = "Timeout";
            }
            catch (Exception ex)
            {
                pushResponse.IsSuccessful = false;
                pushResponse.Body += ex.GetType().Name;
            }

            _logger.LogResponse(requestGrouping, pushResponse.RequestUrl, pushResponse.IsSuccessful,
                pushResponse.ResponseHeaders, pushResponse.TimeTakenMilliseconds, pushResponse.Body);

            return pushResponse;
        }

        private static PushResponse PopulatePushResponse(PushResponse pushResponse, HttpResponseMessage response,
            Stopwatch stoppedStopwatch)
        {
            pushResponse.RequestUrl = response.RequestMessage.RequestUri.ToString();
            pushResponse.TimeTakenMilliseconds = (int) stoppedStopwatch.ElapsedMilliseconds;
            pushResponse.IsSuccessful = (int) response.StatusCode >= 200 && (int) response.StatusCode <= 206;
            pushResponse.ResponseHeaders = response.Headers.ToString();
            pushResponse.Body = response.StatusCode.ToString();
            //pushResponse.Body = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            return pushResponse;
        }
    }
}