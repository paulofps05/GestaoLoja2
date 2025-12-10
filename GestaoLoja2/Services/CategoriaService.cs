using GestaoLoja2.Entitles;

namespace GestaoLoja2.Services
{
    public class CategoriaService
    {
        private readonly HttpClient http;
        public CategoriaService(IHttpClientFactory factory)
        {
            http = factory.CreateClient("api");
        }
        public async Task<IEnumerable<categoria>> GetCategorias()
        {
            return await http.GetFromJsonAsync<IEnumerable<categoria>>("api/categorias");
        }

        public async Task<categoria?> CreateCategoria(categoria cat)
        {
            var response = await http.PostAsJsonAsync("api/categorias", cat);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<categoria>();
            }

            return null;
        }
    }
}