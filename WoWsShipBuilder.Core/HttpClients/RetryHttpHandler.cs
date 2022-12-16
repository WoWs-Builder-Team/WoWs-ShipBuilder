using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Core.HttpClients;

public class RetryHttpHandler : DelegatingHandler
{
    private const int MaxRetries = 3;

    public RetryHttpHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var i = 0; i < MaxRetries - 1; i++)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            Logging.Logger.Info("Http request for uri {0} failed. Retry attempt {1}", request.RequestUri, i + 1);
        }

        Logging.Logger.Warn("Reached maximum number of retry requests for uri {0}. Continuing with last result.", request.RequestUri);
        return await base.SendAsync(request, cancellationToken);
    }
}
