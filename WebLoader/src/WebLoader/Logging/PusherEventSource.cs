using System;
using System.Diagnostics.Tracing;

namespace WebLoader.Logging
{
    [EventSource(Name = "WebLoader-EventLog")]
    public sealed class PusherEventSource : EventSource, IPusherLogger
    {
        public const int StartedEventId = 28001;
        public const int InfoEventId = 28002;
        public const int FinishedEventId = 28003;
        public const int ResponseEventId = 28101;
        public const int RequestGroupSummaryEventId = 28201;

        // Create a static field instance that provides access to an instance of this class
        private static readonly Lazy<PusherEventSource> Instance
            = new Lazy<PusherEventSource>(() => new PusherEventSource());

        private PusherEventSource()
        {
        }

        // Create a static property called Log that returns the current value 
        // of the Instance field of the event source. This value is 
        // determined by the custom event methods called in the library.
        public static PusherEventSource Log => Instance.Value;

        [Event(StartedEventId,
            Message =
                "Started the run for specification with NumberOfSeconds={0}, StartingRequestCount={1}, MaxRequestCount={2}, BaseUrl={3}, DefaultRequestHeaders={4}",
            Level = EventLevel.Informational, Keywords = Keywords.RunDiagnostics)]
        public void LogStarted(int numberOfSeconds, int startingRequestCount, int maxRequestCount, string baseUrl,
            string defaultHeaders)
        {
            if (IsEnabled())
                WriteEvent(StartedEventId, numberOfSeconds, startingRequestCount, maxRequestCount, baseUrl,
                    defaultHeaders);
        }

        [Event(FinishedEventId, Message = "Finished the run",
            Level = EventLevel.Informational, Keywords = Keywords.RunDiagnostics)]
        public void LogFinished()
        {
            if (IsEnabled()) WriteEvent(FinishedEventId);
        }

        [Event(ResponseEventId,
            Message = "{0} Received response from {1} with successful={2}, headers={3}, {4}ms taken, body={5}",
            Level = EventLevel.Informational, Keywords = Keywords.RequestResponse)]
        public void LogResponse(int requestGrouping, string requestUrl, bool isSuccessful, string responseHeaders,
            int timeTaken, string body)
        {
            if (IsEnabled())
                WriteEvent(ResponseEventId, requestGrouping, requestUrl, isSuccessful, responseHeaders, timeTaken, body);
        }

        [Event(RequestGroupSummaryEventId,
            Message = "{0} Completed group ({1}/{2} successful with {3}ms average time taken",
            Level = EventLevel.Informational, Keywords = Keywords.GroupingSummary)]
        public void LogRequestGroupingSummary(int requestGrouping, int successful, int total, int averageTimeTaken)
        {
            if (IsEnabled())
                WriteEvent(RequestGroupSummaryEventId, requestGrouping, successful, total, averageTimeTaken);
        }

        [Event(InfoEventId, Message = "{0}",
            Level = EventLevel.Informational, Keywords = Keywords.RunDiagnostics)]
        public void LogInfo(string infoMessage)
        {
            if (IsEnabled()) WriteEvent(InfoEventId, infoMessage);
        }

        public class Keywords
        {
            public const EventKeywords RunDiagnostics = (EventKeywords) 1;
            public const EventKeywords RequestResponse = (EventKeywords) 2;
            public const EventKeywords GroupingSummary = (EventKeywords) 4;
        }
    }
}