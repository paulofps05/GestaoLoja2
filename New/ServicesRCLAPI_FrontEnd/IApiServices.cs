using RCLAPI.DTO;

namespace RCLAPI.Services;
public interface IApiServices
{
    // Comunicar com a API
    public Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint);
    public Task<List<Categoria>> GetCategorias();
    public Task<ApiResponse<List<ProdutoFavorito>>> GetFavoritos();

    // Obter Produtos
    public Task<(List<ProdutoDTO>? Produtos, string? ErrorMessage)> GetProdutos(string tipoProduto, 
        string categoriaId);
    public Task<ProdutoDTO> GetDetalheProduto(int IdProduto);

    public Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria);

    // Gerir Users
    public Task<ApiResponse<bool>> RegistarUtilizador(RegisterModel registerModel);
    public Task<ApiResponse<bool>> Login(LoginModel login);

    public Task<ApiResponse<UtilizadorEstadoDTO>> VerificarEstadoUtilizador(string id);



    // Gerir Carrinho de Compras
    public Task<(bool Data, string? ErrorMessage)> ActualizaQuantidadeItemCarrinho(int produtoId, 
        string acao);
    public Task<ApiResponse<bool>> AdicionaItemNoCarrinho(ItemCarrinhoDTO carrinhoCompra);
    public Task<ApiResponse<List<ItemCarrinhoDTO>>> GetCarrinhoCompras();
    public Task<ApiResponse<bool>> AtualizarQuantidadeProdutoCarrinho(int produtoId, 
        double novaQuantidade);
    public Task<ApiResponse<bool>> AdicionarProdutoCarrinho(ItemCarrinhoDTO itemCarrinhoCompra);
    public Task<ApiResponse<bool>> RemoverProdutoCarrinho(int produtoId);
 


    // Gerir Favoritos
    public Task<ApiResponse<bool>> AdicionarFavorito(int produtoId);
    public Task<(bool Data, string? ErrorMessage)> ActualizaFavorito(string acao, int produtoId);
    public Task<ApiResponse<bool>> RemoverFavorito(int produtoId);
}
