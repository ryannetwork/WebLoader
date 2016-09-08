using WebLoader.Logging;

namespace WebLoader.Http
{
    /// <summary>
    ///     Simple factory to provide an instance of a HTTP gateway for a run specification.
    /// </summary>
    public interface IHttpGatewayProvider
    {
        IHttpGateway Generate(IRunSpecification runSpec);
    }

    public class HttpGatewayProvider : IHttpGatewayProvider
    {
        private readonly IPusherLogger _logger;

        public HttpGatewayProvider(IPusherLogger logger)
        {
            _logger = logger;
        }

        public IHttpGateway Generate(IRunSpecification runSpec)
        {
            return new HttpGateway(_logger, runSpec.BaseUrl, runSpec.RequestTimeout, runSpec.DefaultRequestHeaders);
        }
    }
}