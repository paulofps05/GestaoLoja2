using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CategoriaRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<categoria>> GetCategorias()
        {
            return await dbContext.Categorias
                .Where(x => x.Imagem.Length > 0)
                .OrderBy(O => O.Ordem)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }       
    }
}
