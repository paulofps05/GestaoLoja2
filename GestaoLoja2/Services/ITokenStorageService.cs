using GestaoLoja2.DTO;

namespace GestaoLoja2.Services;

public interface ITokenStorageService
{
    Task<Token?> GetToken();
    Task Login(Token token);
    Task Logout();
    Task<bool> IsUserLoggedIn();
}
