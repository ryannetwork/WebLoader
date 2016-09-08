using System;
using System.Collections.Generic;

namespace WebLoader
{
    public interface IRunSpecification
    {
        int NumberOfSeconds { get; }

        int StartingRequestCount { get; }

        int MaxRequestCount { get; }

        string BaseUrl { get; }

        int RequestTimeout { get; }

        string Verb { get; }

        string Content { get; }
        IDictionary<string, string> DefaultRequestHeaders { get; }

        string GenerateRelativeUrl();
    }

    public class BasicRunSpecification : IRunSpecification
    {
        public BasicRunSpecification(
            int numberOfSeconds,
            int startingRequestCount,
            int maxRequestCount,
            string baseUrl,
            int requestTimeout,
            string verb,
            string content,
            IDictionary<string, string> requestHeaders = null)
        {
            ValidateSpecificationInvariants(numberOfSeconds, startingRequestCount, maxRequestCount, baseUrl,
                requestTimeout);

            NumberOfSeconds = numberOfSeconds;
            StartingRequestCount = startingRequestCount;
            MaxRequestCount = maxRequestCount;
            BaseUrl = baseUrl;
            RequestTimeout = requestTimeout;
            Verb = verb;
            Content = content;
            DefaultRequestHeaders = requestHeaders ?? new Dictionary<string, string>();
        }

        public int NumberOfSeconds { get; }

        public int StartingRequestCount { get; }

        public int MaxRequestCount { get; }

        public string BaseUrl { get; }

        public int RequestTimeout { get; }

        public string Verb { get; }

        public string Content { get; }

        public IDictionary<string, string> DefaultRequestHeaders { get; }

        public virtual string GenerateRelativeUrl() => string.Empty;

        private static void ValidateSpecificationInvariants(
            int numberOfSeconds,
            int startingRequestCount,
            int maxRequestCount,
            string baseUrl,
            int requestTimeout)
        {
            if (numberOfSeconds <= 0)
                throw new ArgumentException("numberOfSeconds cannot be zero/negative");
            if (numberOfSeconds > SpecificationConstants.MaxTimeDuration)
                throw new ArgumentException(
                    $"numberOfSeconds must be under limit of {SpecificationConstants.MaxTimeDuration}");

            if (startingRequestCount < 0)
                throw new ArgumentException("startingRequestCount cannot be negative");
            if (startingRequestCount > SpecificationConstants.MaxStartingRequestsPerSecond)
                throw new ArgumentException(
                    $"startingRequestCount must be under limit of {SpecificationConstants.MaxStartingRequestsPerSecond}");

            if (maxRequestCount <= 0)
                throw new ArgumentException("maxRequestCount cannot be zero/negative");
            if (startingRequestCount > SpecificationConstants.MaxCappedRequestPerSecond)
                throw new ArgumentException(
                    $"maxRequestCount must be under limit of {SpecificationConstants.MaxCappedRequestPerSecond}");

            if (requestTimeout <= 0)
                throw new ArgumentException("requestTimeout cannot be zero/negative");
            if (requestTimeout > SpecificationConstants.MaxTimeout)
                throw new ArgumentException($"requestTimeout must be under limit of {SpecificationConstants.MaxTimeout}");

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl cannot be null or whitespace");

            Uri uri;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out uri))
                throw new ArgumentException("baseUrl must be a valid URL");
        }
    }
}