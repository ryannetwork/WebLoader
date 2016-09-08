namespace WebLoader
{
    public static class SpecificationConstants
    {
        /// <summary>
        ///     Maximum time duration allowed for run in seconds. (5 hours)
        /// </summary>
        public const int MaxTimeDuration = 18000;

        /// <summary>
        ///     Default requests/second for the first interval.
        /// </summary>
        public const int DefaultStartingRequestsPerSecond = 10;

        /// <summary>
        ///     Maximum allowed requests/second for the first interval.
        /// </summary>
        public const int MaxStartingRequestsPerSecond = 50000;

        /// <summary>
        ///     Default maximum requests/second capped across the whole the run.
        /// </summary>
        public const int DefaultMaxCappedRequestsPerSecond = 100;

        /// <summary>
        ///     Maximum requests/second capped across the whole the run.
        /// </summary>
        public const int MaxCappedRequestPerSecond = 50000;

        /// <summary>
        ///     Minimum duration in milliseconds for the request to timeout.
        /// </summary>
        public const int MinTimeout = 100;

        /// <summary>
        ///     Default duration in milliseconds before the request times out.
        /// </summary>
        public const int DefaultTimeout = 1000;

        /// <summary>
        ///     Max duration in milliseconds before the request times out.
        /// </summary>
        public const int MaxTimeout = 1000;

        /// <summary>
        ///     Maximum requests over run (requests/second * duration) allowed for logging to file to be enabled. 3,000,000
        /// </summary>
        public const int MaxTotalRequestsAllowedForResponseLogging = 3000000;
    }
}