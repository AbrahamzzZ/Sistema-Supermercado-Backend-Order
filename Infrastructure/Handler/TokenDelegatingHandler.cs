using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Infrastructure.Handler
{
    public class TokenDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"]
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    AuthenticationHeaderValue.Parse(token);
            }

            Console.WriteLine(request.Headers.Authorization);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
