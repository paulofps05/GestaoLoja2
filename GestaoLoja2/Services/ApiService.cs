using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GestaoLoja2.DTO;
using GestaoLoja2.Entitles;

namespace GestaoLoja2.Services;

public class ApiService : IApiServices
{
    private readonly ILogger<ApiService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly JsonSerializerOptions _serializerOptions;

    public ApiService(ILogger<ApiService> logger, HttpClient httpClient, ITokenStorageService tokenStorageService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
        _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task AddAuthorizationHeader()
    {
        var token = await _tokenStorageService.GetToken();
        if (token != null && !string.IsNullOrEmpty(token.AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }
    }

    public async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint)
    {
        try
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions);
                return (data, null);
            }
            return (default, $"Erro: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no GetAsync");
            return (default, ex.Message);
        }
    }

    public async Task<List<categoria>> GetCategorias()
    {
        var (data, error) = await GetAsync<List<categoria>>("api/categorias");
        return data ?? new List<categoria>();
    }

    public async Task<(List<ProdutoDTO>? Produtos, string? ErrorMessage)> GetProdutos(string tipoProduto, string categoriaId)
    {
        return await GetAsync<List<ProdutoDTO>>($"api/produtos?tipoProduto={tipoProduto}&categoriaId={categoriaId}");
    }

    public async Task<ProdutoDTO> GetDetalheProduto(int IdProduto)
    {
        var (data, error) = await GetAsync<ProdutoDTO>($"api/produtos/{IdProduto}");
        return data;
    }

    public async Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria)
    {
        string endpoint = produtoTipo switch
        {
            "promocao" => "api/produtos/Promos",
            "maisvendido" => "api/produtos/bestSellers",
            "categoria" => $"api/produtos/byCategory?id={IdCategoria}",
            _ => "api/produtos"
        };

        var (data, error) = await GetAsync<List<ProdutoDTO>>(endpoint);
        return data ?? new List<ProdutoDTO>();
    }

    public async Task<ApiResponse<bool>> RegistarUtilizador(RegisterModel registerModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(registerModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/RegistarUser", content);

            return new ApiResponse<bool> { Data = response.IsSuccessStatusCode, ErrorMessage = response.IsSuccessStatusCode ? null : "Erro ao registar" };
        }
        catch (Exception ex) { return new ApiResponse<bool> { ErrorMessage = ex.Message }; }
    }

    public async Task<ApiResponse<bool>> Login(LoginModel login)
    {
        try
        {
            var json = JsonSerializer.Serialize(login);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/LoginUser", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<Token>(responseString, _serializerOptions);
                if (token != null)
                {
                    await _tokenStorageService.Login(token);
                    return new ApiResponse<bool> { Data = true };
                }
            }
            return new ApiResponse<bool> { Data = false, ErrorMessage = "Login inválido" };
        }
        catch (Exception ex) { return new ApiResponse<bool> { ErrorMessage = ex.Message }; }
    }

    public async Task<ApiResponse<UtilizadorEstadoDTO>> VerificarEstadoUtilizador(string id)
    {
        var (data, error) = await GetAsync<UtilizadorEstadoDTO>($"api/Auth/CheckIfActive?id={id}");
        return new ApiResponse<UtilizadorEstadoDTO> { Data = data, ErrorMessage = error };
    }
}