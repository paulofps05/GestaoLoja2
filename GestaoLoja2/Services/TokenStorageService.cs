using Blazored.LocalStorage;
using GestaoLoja2.DTO;
using Microsoft.JSInterop;

namespace GestaoLoja2.Services;

public class TokenStorageService : ITokenStorageService
{
    private readonly ILocalStorageService _localStorage;
    private const string TokenKey = "authToken";

    public TokenStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<Token?> GetToken()
    {
        try 
        {
            return await _localStorage.GetItemAsync<Token>(TokenKey);
        }
        catch (Exception)
        {
            // Ignorar erros durante o prerendering (ex: JS Interop não disponível)
            return null;
        }
    }

    public async Task Login(Token token)
    {
        try
        {
            await _localStorage.SetItemAsync(TokenKey, token);
        }
        catch (Exception)
        {
            // Ignorar erros se tentado chamar durante o prerendering.
            // Nota: Num cenário real de login interativo, isto não deve acontecer se o botão estiver num componente InteractiveServer.
        }
    }

    public async Task Logout()
    {
        try
        {
            await _localStorage.RemoveItemAsync(TokenKey);
        }
        catch (Exception)
        {
            // Ignorar erros
        }
    }

    public async Task<bool> IsUserLoggedIn()
    {
        var token = await GetToken();
        return token != null && !string.IsNullOrEmpty(token.AccessToken);
    }
}