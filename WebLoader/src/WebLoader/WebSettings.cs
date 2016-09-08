using System.Collections.Generic;
using System.ComponentModel;

namespace WebLoader
{
    public class WebSettings : IWebSettings
    {
        public WebSettings()
        {
            Url = "http://localhost:5000/api/values";
            TimeDuration = 60;
            StartCount = 10;
            MaxRequestCount = 20;
            Timeout = 1000;
        }

        public List<UrlHeader> UrlHeaders { get; set; }

        [Description("Required: The string Content Encode in Base64")]
        public string Content { get; set; }

        [Description("Required: The Action Verb")]
        public string Verb { get; set; }

        [Description("Required: The base URL to run against")]
        public string Url { get; set; }

        [Description("Required: The time duration of run in seconds")]
        public int TimeDuration { get; set; }

        [Description("Optional: The starting number of requests/second")]
        public int StartCount { get; set; }


        [Description("Optional: The maximum number of requests/second")]
        public int MaxRequestCount { get; set; }


        [Description("Optional: The timeout in milliseconds")]
        public int Timeout { get; set; }

        private void SetDefaultValues()
        {
            if (StartCount == 0)
            {
                StartCount = SpecificationConstants.DefaultStartingRequestsPerSecond;
            }

            if (MaxRequestCount == 0)
            {
                MaxRequestCount = SpecificationConstants.DefaultMaxCappedRequestsPerSecond;
            }

            if (Timeout == 0)
            {
                Timeout = SpecificationConstants.DefaultTimeout;
            }
        }
    }
}