using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

            Logging.Logger.LogInformation("Http request for uri {RequestUri} failed. Retry attempt {Attempt}", request.RequestUri, i + 1);
        }

        Logging.Logger.LogWarning("Reached maximum number of retry requests for uri {RequestUri}. Continuing with last result", request.RequestUri);
        return await base.SendAsync(request, cancellationToken);
    }
}
