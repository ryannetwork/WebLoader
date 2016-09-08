using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using WebLoader.Http;
using WebLoader.Logging;

namespace WebLoader.ConsoleApp
{
    public class Application
    {
        private readonly ILogger _logger;
        private readonly WebSettings _webSettings;

        public Application(ILogger<Application> logger, IOptions<WebSettings> webSettings)
        {
            _logger = logger;
            _webSettings = webSettings.Value;
        }

        public void Run()
        {
            try
            {
                var cts = SetupUserInputCancellationTokenSource();
                var logger = PusherEventSource.Log;
                var loggingEventListeners = SetupLoggingEventListeners(_webSettings, logger).ToList();

                var urlHeaders = new Dictionary<string, string>();
                var contentType = string.Empty;
                foreach (var item in _webSettings.UrlHeaders)
                {
                    urlHeaders.Add(item.Name, item.Value);
                }

                string content = null;

                if (_webSettings.Content != null)
                {
                    byte[] data = Convert.FromBase64String(_webSettings.Content);
                    content = Encoding.UTF8.GetString(data);
                }


                var pusher = new LoadPusher(new HttpGatewayProvider(logger), logger);
                var spec = new BasicRunSpecification(
                    _webSettings.TimeDuration,
                    _webSettings.StartCount,
                    _webSettings.MaxRequestCount,
                    _webSettings.Url,
                    _webSettings.Timeout,
                    _webSettings.Verb,
                    content,
                    urlHeaders);

                try
                {
                    pusher.PushLoadAsync(spec, cts.Token).Wait(cts.Token);
                    Console.ReadLine();
                }
                finally
                {
                    DisableLoggingEventListeners(loggingEventListeners, logger);
                }


                Console.WriteLine("\nPress <Enter> to terminate.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }


        private static IEnumerable<ObservableEventListener> SetupLoggingEventListeners(WebSettings args,
            PusherEventSource pusherEventSource)
        {
            // Log run diagnostics and request group summaries to console
            // Using custom Observer instead of the verbose consoleLoggingListener.LogToConsole();
            var consoleLoggingListener = new ObservableEventListener();
            consoleLoggingListener.Subscribe(
                Observer.Create<EventEntry>(entry => Console.WriteLine(entry.FormattedMessage)));
            consoleLoggingListener.EnableEvents(
                pusherEventSource,
                EventLevel.Informational,
                PusherEventSource.Keywords.RunDiagnostics | PusherEventSource.Keywords.GroupingSummary);

            yield return consoleLoggingListener;

            // Log all responses to rolling flat file only if total requests for run is under limit
            var totalRequestsExpectedOverRun = Enumerable.Range(args.StartCount, args.TimeDuration)
                .Select(requestCount => requestCount > args.MaxRequestCount
                    ? args.MaxRequestCount // Factor in capped maximum requests/second
                    : requestCount)
                .Sum();
            if (totalRequestsExpectedOverRun > SpecificationConstants.MaxTotalRequestsAllowedForResponseLogging)
            {
                pusherEventSource.LogInfo(
                    $@"Note: Total request count {totalRequestsExpectedOverRun} 
                            exceeds limit of {SpecificationConstants
                        .MaxTotalRequestsAllowedForResponseLogging} to allow logging of all responses to file.");
                yield break;
            }

            var flatFileLoggingListener = new ObservableEventListener();


            flatFileLoggingListener.EnableEvents(
                pusherEventSource,
                EventLevel.Informational,
                PusherEventSource.Keywords.RequestResponse | PusherEventSource.Keywords.GroupingSummary);

            yield return flatFileLoggingListener;
        }

        private static void DisableLoggingEventListeners(IEnumerable<ObservableEventListener> listeners,
            EventSource logger)
        {
            foreach (var listener in listeners)
            {
                listener.DisableEvents(logger);
                //listener.Dispose(); // causes it to hang
            }
        }

        private static CancellationTokenSource SetupUserInputCancellationTokenSource()
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            return cts;
        }
    }
}