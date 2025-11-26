using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Repositories
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<categoria>> GetCategorias();
    }
}
