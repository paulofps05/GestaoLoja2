using GestaoLoja2.Components.Account.Pages.Manage;
using GestaoLoja2.Entitles;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Text.Json;

namespace GestaoLoja2.Services
{
    public class AutenticacaoService
    {
        private readonly HttpClient http;
        private readonly TokenStorageService _tokenStorage;

        public AutenticacaoService(
            IHttpClientFactory factory,
            TokenStorageService tokenStorage)
        {
            http = factory.CreateClient("api");
            _tokenStorage = tokenStorage;
        }

        public async Task<LoginResult> Login(string email, string password)
        {
            var loginData = new
            {
                Email = email,
                Password = password
            };

            try
            {
                var result = await http.PostAsJsonAsync("/Identity/Login", loginData);

                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                    {
                        _tokenStorage.SetToken(authResponse.AccessToken, authResponse.ExpiresIn);

                        Console.WriteLine("----------------");
                        Console.WriteLine("Login efetuado com sucesso!");
                        Console.WriteLine("Token recebido: " + authResponse.AccessToken.Substring(0, 20) + "...");
                        Console.WriteLine("Tipo de token: {0}", authResponse.TokenType);
                        Console.WriteLine("Expira em (s): {0}", authResponse.ExpiresIn);
                        Console.WriteLine("Email: {0}", authResponse.Email);
                        Console.WriteLine("----------------");
                        return new LoginResult
                        {
                            Success = true,
                            Message = "Login efetuado com sucesso!",
                            Email = authResponse.Email
                        };
                    }
                }
                return new LoginResult
                {
                    Success = false,
                    Message = "Email ou password inválidos",
                    Email = string.Empty
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no login: " + ex.Message);
                return new LoginResult
                {
                    Success = false,
                    Message = "Erro ao efetuar login: " + ex.Message,
                    Email = string.Empty
                };
            }
        }

        public class AuthResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string TokenType { get; set; } = string.Empty;
            public int ExpiresIn { get; set; }
            public string Password { get; set; } = string.Empty;

            // Used by Login() logging and result payload
            public string Email { get; set; } = string.Empty;
        }

        public class LoginResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}
