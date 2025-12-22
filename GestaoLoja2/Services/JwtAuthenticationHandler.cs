using System.Net.Http.Headers;

namespace GestaoLoja2.Services
{
    public class JwtAuthenticationHandler : DelegatingHandler
    {
        private readonly ITokenStorageService _tokenStorage;

        public JwtAuthenticationHandler(ITokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // O GetToken agora é assíncrono e retorna um objeto Token
            var token = await _tokenStorage.GetToken();
            
            if (token != null && !string.IsNullOrEmpty(token.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}