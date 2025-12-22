using GestaoLoja2.DTO;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestaoLoja2.Services
{
    public class AutenticacaoService
    {
        private readonly IApiServices _apiService;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AutenticacaoService(IApiServices apiService, AuthenticationStateProvider authStateProvider)
        {
            _apiService = apiService;
            _authStateProvider = authStateProvider;
        }

        public async Task<LoginResult> Login(string email, string password)
        {
            var loginModel = new LoginModel
            {
                Email = email,
                Password = password
            };

            // O ApiService já trata da comunicação e de guardar o token
            var response = await _apiService.Login(loginModel);

            if (response.Data) // Se Data for true, o login foi sucesso
            {
                // Notificar o provider que o estado mudou
                ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(email);

                return new LoginResult
                {
                    Success = true,
                    Message = "Login efetuado com sucesso!",
                    Email = email
                };
            }

            return new LoginResult
            {
                Success = false,
                Message = response.ErrorMessage ?? "Email ou password inválidos.",
                Email = string.Empty
            };
        }

        // Mantemos a classe LoginResult para compatibilidade com as páginas existentes
        public class LoginResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
