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

        public async Task<categoria?> GetCategoriaById(int id)
        {
            var response = await http.GetAsync($"api/categorias/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<categoria>();
            }
            return null;
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

        public async Task UpdateCategoria(categoria cat)
        {
            await http.PutAsJsonAsync($"api/categorias/{cat.Id}", cat);
        }

        public async Task DeleteCategoria(int id)
        {
            await http.DeleteAsync($"api/categorias/{id}");
        }
    }
}