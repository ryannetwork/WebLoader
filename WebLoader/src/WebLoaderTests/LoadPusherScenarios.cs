using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WebLoader;
using WebLoader.Http;
using WebLoader.Logging;
using Xunit;

namespace WebLoaderTests
{
    public class LoadPusherScenarios
    {
        private readonly ILoadPusher loadPusher;
        private readonly Mock<IHttpGatewayProvider> mockHttpGatewayProvider;
        private readonly Mock<IPusherLogger> mockPusherLogger;

        public LoadPusherScenarios()
        {
            mockPusherLogger = new Mock<IPusherLogger>();
            mockPusherLogger.Setup(
                logger =>
                    logger.LogStarted(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                        It.IsAny<string>()));
            mockPusherLogger.Setup(logger => logger.LogFinished());
            mockPusherLogger.Setup(
                logger =>
                    logger.LogResponse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(),
                        It.IsAny<int>(), It.IsAny<string>()));
            mockPusherLogger.Setup(
                logger =>
                    logger.LogRequestGroupingSummary(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()));

            mockHttpGatewayProvider = new Mock<IHttpGatewayProvider>();
            mockHttpGatewayProvider.Setup(provider => provider.Generate(It.IsAny<IRunSpecification>()))
                .Returns(new FakeHttpGateway(mockPusherLogger.Object));

            loadPusher = new LoadPusher(mockHttpGatewayProvider.Object, mockPusherLogger.Object);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task Should_Log_All_Grouping_Summaries_For_Basic_Specification(int timeDuration)
        {
            var spec = new BasicRunSpecification(timeDuration, 100, 200, "http://localhost/", 1000, "GET", null);

            await loadPusher.PushLoadAsync(spec, CancellationToken.None);

            mockPusherLogger.Verify(
                logger =>
                    logger.LogRequestGroupingSummary(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Exactly(timeDuration),
                "LogRequestGroupingSummary should be called the same number of times as the duration of the load run.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task Should_Trigger_Incrementing_Requests_For_Basic_Specification(int timeDuration)
        {
            var spec = new BasicRunSpecification(timeDuration, 100, 200, "http://localhost/", 1000, "GET", null);

            await loadPusher.PushLoadAsync(spec, CancellationToken.None);

            var expectedResponsesCount = Enumerable.Range(100, timeDuration).Sum();

            //mockPusherLogger.Verify(
            //    logger =>
            //        logger.LogResponse(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(),
            //            It.IsAny<int>(), It.IsAny<string>()),
            //    Times.Exactly(expectedResponsesCount),
            //    "LogResponse should be called the total number of times as the summation of the starting count with the incrementing duration of the load run.");
        }
    }

    public class FakeHttpGateway : IHttpGateway
    {
        private const string BaseUrl = "http://localhost/";
        private static readonly Random random = new Random();
        private readonly IPusherLogger _logger;

        public FakeHttpGateway(IPusherLogger logger)
        {
            _logger = logger;
        }

        public async Task<PushResponse> GetDelayedAsync(string requestUrl, string verb, string content,
            int staggeredDelay, int requestGrouping,
            CancellationToken token)
        {
            await Task.Delay(staggeredDelay, token);

            _logger.LogResponse(requestGrouping, BaseUrl + requestUrl, true, "", staggeredDelay, "");

            return new PushResponse
            {
                IsSuccessful = true,
                RequestUrl = BaseUrl + requestUrl,
                RequestGrouping = requestGrouping,
                RequestTime = DateTime.UtcNow,
                TimeTakenMilliseconds = staggeredDelay
            };
        }

        public async Task<IEnumerable<PushResponse>> MakeStaggeredRequests(IRunSpecification runSpec, int requestCount,
            int requestGrouping, CancellationToken token)
        {
            var delayTasks = new List<Task<PushResponse>>();
            for (var count = 0; count < requestCount; count++)
            {
                var delay = random.Next(1, 1000);
                delayTasks.Add(GetDelayedAsync(runSpec.GenerateRelativeUrl(), "GET", null, delay, count,
                    CancellationToken.None));
            }

            return await Task.WhenAll(delayTasks).ConfigureAwait(false);
        }
    }
}