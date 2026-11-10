using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Infrastructure.Handler
{
    public class TokenDelegatingHandler : DelegatingHandler
    {
        private const string CorrelationHeader = "X-Correlation-Id";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var token = httpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
            }

            if (httpContext?.Items["CorrelationId"] is string correlationId && !request.Headers.Contains(CorrelationHeader))
            {
                request.Headers.TryAddWithoutValidation(CorrelationHeader, correlationId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
