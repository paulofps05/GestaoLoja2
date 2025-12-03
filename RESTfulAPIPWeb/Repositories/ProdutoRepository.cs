using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {

        public readonly ApplicationDbContext _dbContext;

        public ProdutoRepository(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<IEnumerable<produto>> GetProdutosPorCategoriaAsync(int categoriaId)
        {
            return await _dbContext.Produtos
                .Where(x => x.CategoriaId == categoriaId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutosPromocaoAsync()
        {
            return await _dbContext.Produtos
                .Where(x => x.Promocao == true)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutosMaisVendidosAsync()
        {
            return await _dbContext.Produtos
                .Where(x => x.MaisVendido == true)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutoDetalhesAsync(int produtoId)
        {
            return await _dbContext.Produtos
                .Where(x => x.Id == produtoId)
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<produto>> GetProdutosAsync()
        {
            return await _dbContext.Produtos
                .OrderBy(p => p.Nome)
                .ThenBy(p => p.categoria)
                .ToListAsync();
        }

        public async Task<produto> AdicionarProdutosAsync(produto produto)
        {
            // O código original estava correto
            _dbContext.Produtos.Add(produto);
            await _dbContext.SaveChangesAsync();
            return produto;
        }

        public async Task<bool> UpdateProdutosAsync(int id, produto produto)
        {
            // CORREÇÃO: Verificar existência sem fazer "Tracking" para evitar conflito de IDs
            var existe = await _dbContext.Produtos.AsNoTracking().AnyAsync(x => x.Id == id);

            if (!existe) return false;

            _dbContext.Produtos.Update(produto);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProdutosAsync(int id)
        {
            // O código original estava correto
            var produto = await _dbContext.Produtos.FindAsync(id);
            if (produto is null) return false;

            _dbContext.Produtos.Remove(produto);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // NOVO: Implementação da verificação
        public async Task<bool> ProdutoExisteAsync(int id)
        {
            return await _dbContext.Produtos.AnyAsync(e => e.Id == id);
        }
    }
}
