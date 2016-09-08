using System;

namespace WebLoader
{
    public class PushResponse
    {
        public int RequestGrouping { get; set; }

        public DateTime RequestTime { get; set; }

        public string RequestUrl { get; set; }

        public bool IsSuccessful { get; set; }

        public string ResponseHeaders { get; set; }

        public int TimeTakenMilliseconds { get; set; }

        public string Body { get; set; }
    }
}