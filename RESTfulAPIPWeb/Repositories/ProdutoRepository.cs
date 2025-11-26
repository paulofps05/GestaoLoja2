using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {

        public readonly ApplicationDbContext dbContext;

        public ProdutoRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<produto>> GetProdutosPorCategoriaAsync(int categoriaId)
        {
            return await dbContext.Produtos
                .Where(x => x.CategoriaId == categoriaId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutosPromocaoAsync()
        {
            return await dbContext.Produtos
                .Where(x => x.Promocao == true)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutosMaisVendidosAsync()
        {
            return await dbContext.Produtos
                .Where(x => x.MaisVendido == true)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutoDetalhesAsync(int produtoId)
        {
            return await dbContext.Produtos
                .Where(x => x.Id == produtoId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutosAsync()
        {
            return await dbContext.Produtos
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }
    }
}
