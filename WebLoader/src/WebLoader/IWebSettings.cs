namespace WebLoader
{
    public interface IWebSettings
    {
        string Url { get; set; }

        int TimeDuration { get; set; }

        int StartCount { get; set; }

        int MaxRequestCount { get; set; }

        int Timeout { get; set; }
    }
}