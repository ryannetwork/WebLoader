namespace WebLoader.Logging
{
    /// <summary>
    ///     Contract for a logger capturing semantic information about a load run.
    /// </summary>
    public interface IPusherLogger
    {
        void LogStarted(int numberOfSeconds, int startingRequestCount, int maxRequestCount, string baseUrl,
            string defaultHeaders);

        void LogFinished();

        void LogResponse(int requestGrouping, string requestUrl, bool isSuccessful, string responseHeaders,
            int timeTaken, string body);

        void LogRequestGroupingSummary(int requestGrouping, int successful, int total, int averageTimeTaken);
    }
}