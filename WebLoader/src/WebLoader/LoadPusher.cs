using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebLoader.Http;
using WebLoader.Logging;

namespace WebLoader
{
    public interface ILoadPusher
    {
        void PushLoad(IRunSpecification runSpec, CancellationToken token);

        Task PushLoadAsync(IRunSpecification runSpec, CancellationToken token);
    }

    public class LoadPusher : ILoadPusher
    {
        private static readonly TimeSpan PerSecondSpan = TimeSpan.FromMilliseconds(1000);
        private readonly IHttpGatewayProvider _httpGatewayProvider;
        private readonly IPusherLogger _logger;

        public LoadPusher(IHttpGatewayProvider httpGatewayProvider, IPusherLogger logger)
        {
            _httpGatewayProvider = httpGatewayProvider;
            _logger = logger;
        }

        public void PushLoad(IRunSpecification runSpec, CancellationToken token)
        {
            _logger.LogStarted(runSpec.NumberOfSeconds, runSpec.StartingRequestCount, runSpec.MaxRequestCount,
                runSpec.BaseUrl, string.Join(",", runSpec.DefaultRequestHeaders));

            // Use the rx observable to make incrementing number of requests (per second) and compose responses
            GenerateObservableForMakingIncrementingRequests(runSpec, token)
                .Subscribe(
                    // OnNext
                    responses =>
                    {
                        // Fold responses to get summaries
                        var groupSummary = responses.Aggregate(
                            new
                            {
                                Group = 0,
                                TotalCount = 0,
                                SuccessCount = 0,
                                TotalTimeTakenMilliseconds = 0
                            },
                            (summary, response) =>
                                new
                                {
                                    Group = response.RequestGrouping,
                                    TotalCount = summary.TotalCount + 1,
                                    SuccessCount = summary.SuccessCount + (response.IsSuccessful ? 1 : 0),
                                    TotalTimeTakenMilliseconds =
                                        summary.TotalTimeTakenMilliseconds + response.TimeTakenMilliseconds
                                });

                        _logger.LogRequestGroupingSummary(
                            groupSummary.Group,
                            groupSummary.SuccessCount,
                            groupSummary.TotalCount,
                            groupSummary.TotalTimeTakenMilliseconds/groupSummary.TotalCount);
                    },
                    // OnComplete
                    () => { _logger.LogFinished(); }, token);
        }

        public async Task PushLoadAsync(IRunSpecification runSpec, CancellationToken token)
        {
            _logger.LogStarted(runSpec.NumberOfSeconds, runSpec.StartingRequestCount, runSpec.MaxRequestCount,
                runSpec.BaseUrl, string.Join(",", runSpec.DefaultRequestHeaders));

            // Use the rx observable to make incrementing number of requests (per second) and compose responses
            var observableInterval = GenerateObservableForMakingIncrementingRequests(runSpec, token);

            await observableInterval
                .ForEachAsync(responses =>
                {
                    // Fold responses to get summaries
                    var groupSummary = responses.Aggregate(
                        new
                        {
                            Group = 0,
                            TotalCount = 0,
                            SuccessCount = 0,
                            TotalTimeTakenMilliseconds = 0
                        },
                        (summary, response) =>
                            new
                            {
                                Group = response.RequestGrouping,
                                TotalCount = summary.TotalCount + 1,
                                SuccessCount = summary.SuccessCount + (response.IsSuccessful ? 1 : 0),
                                TotalTimeTakenMilliseconds =
                                    summary.TotalTimeTakenMilliseconds + response.TimeTakenMilliseconds
                            });

                    _logger.LogRequestGroupingSummary(
                        groupSummary.Group,
                        groupSummary.SuccessCount,
                        groupSummary.TotalCount,
                        groupSummary.TotalTimeTakenMilliseconds/groupSummary.TotalCount);
                }, token)
                .ConfigureAwait(false);

            _logger.LogFinished();
        }

        private IObservable<IEnumerable<PushResponse>> GenerateObservableForMakingIncrementingRequests(
            IRunSpecification runSpec,
            CancellationToken token)
        {
            var httpGateway = _httpGatewayProvider.Generate(runSpec);

            return Observable.Interval(PerSecondSpan)
                // Derive the incrementing (capped) request count from current interval
                .Select(currentInterval => new
                {
                    RequestCount =
                        GetCappedRequestCount((int) currentInterval, runSpec.StartingRequestCount,
                            runSpec.MaxRequestCount),
                    Interval = (int) currentInterval
                })
                // Pipeline request count and current interval into making requests 
                .SelectMany(
                    async requestInfo =>
                        await
                            httpGateway.MakeStaggeredRequests(runSpec, requestInfo.RequestCount, requestInfo.Interval,
                                token)
                                .ConfigureAwait(false))
                // Take interval sequence for specified number of seconds
                .Take(runSpec.NumberOfSeconds);
        }

        private static int GetCappedRequestCount(int currentIncrement, int startingRequestCount, int maxRequestCount)
        {
            // Ensure incrementing request count stays below or equal maxRequestCount
            var attemptedRequestCount = currentIncrement + startingRequestCount;
            return attemptedRequestCount > maxRequestCount
                ? maxRequestCount
                : attemptedRequestCount;
        }
    }
}