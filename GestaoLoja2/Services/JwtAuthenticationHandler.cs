using System.Net.Http.Headers;

namespace GestaoLoja2.Services
{
    public class JwtAuthenticationHandler : DelegatingHandler
    {
        private readonly TokenStorageService _tokenStorage;

        public JwtAuthenticationHandler(TokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _tokenStorage.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await base.SendAsync(request, cancellationToken);
        }


    }
}
