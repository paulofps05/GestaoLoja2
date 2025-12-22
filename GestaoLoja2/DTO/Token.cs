namespace GestaoLoja2.DTO;

public class Token
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public string UtilizadorNome { get; set; } = string.Empty;
    public string UtilizadorId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}