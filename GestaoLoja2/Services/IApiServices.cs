using GestaoLoja2.DTO;
using GestaoLoja2.Entitles;

namespace GestaoLoja2.Services;

public interface IApiServices
{
    Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint);
    Task<List<categoria>> GetCategorias();
    Task<(List<ProdutoDTO>? Produtos, string? ErrorMessage)> GetProdutos(string tipoProduto, string categoriaId);
    Task<ProdutoDTO> GetDetalheProduto(int IdProduto);
    Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria);
    
    Task<ApiResponse<bool>> RegistarUtilizador(RegisterModel registerModel);
    Task<ApiResponse<bool>> Login(LoginModel login);
    Task<ApiResponse<UtilizadorEstadoDTO>> VerificarEstadoUtilizador(string id);
}