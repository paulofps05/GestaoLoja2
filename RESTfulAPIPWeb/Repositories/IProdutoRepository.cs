using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Repositories
{
    public interface IProdutoRepository
    {
        Task<IEnumerable<produto>> GetProdutosPorCategoriaAsync(int categoriaId);
        Task<IEnumerable<produto>> GetProdutosPromocaoAsync();
        Task<IEnumerable<produto>> GetProdutosMaisVendidosAsync();
        Task<IEnumerable<produto>> GetProdutoDetalhesAsync(int produtoId);
        Task<IEnumerable<produto>> GetProdutosAsync();

    }
}
