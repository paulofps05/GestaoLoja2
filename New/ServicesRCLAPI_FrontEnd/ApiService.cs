using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RCLAPI.DTO;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RCLAPI.Services;
public class ApiService : IApiServices
{ 
    // Dependency Service Injection Variables
    private readonly ILogger<ApiService> _logger;
    private readonly HttpClient _httpClient = new(); 
    private readonly IUserSessionService _userSessionService;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Products List & Promo Product
    private List<ProdutoDTO> produtos;
    private List<Categoria> categorias;
    private ProdutoDTO detalhesProduto;
    public ApiService(IHttpContextAccessor httpContextAccessor, ILogger<ApiService> logger, 
        HttpClient httpClient, IUserSessionService userSessionService)
    {
        // Services Dependency Injection 
        _logger = logger;
        _httpClient = httpClient;
        _userSessionService = userSessionService;
        _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _httpContextAccessor = httpContextAccessor;

        // Inicialização das Listas
        produtos = new List<ProdutoDTO>();
        categorias = new List<Categoria>();
        detalhesProduto = new ProdutoDTO();
    }

    // Adicionar o token ao cabeçalho do pedido
    private async Task AddAuthorizationHeader()
    {
        var token = await _userSessionService.GetToken();

        if (token != null && !string.IsNullOrEmpty(token.AccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);
            _logger.LogInformation($"Header Authorization token adicionado {token.AccessToken}.");
        }
        else
        {
            _logger.LogWarning("Token não encontrado ou inválido. O Header Authorization não foi configurado.");
        }
    }

    // General Functions to comunicate with API
    // General Get Function to comunicate with API
    public async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint)
    {
        var enderecoUrl = AppConfig.BaseUrl + endpoint;

        try
        {
            // Adiciona o token ao cabeçalho para autenticação
            await AddAuthorizationHeader();

            // Envia o pedido HTTP à API
            var response = await _httpClient.GetAsync(enderecoUrl);

            if (response.IsSuccessStatusCode)
            {
                // Obtém o conteúdo enviado na resposta ao pedido HTTP à API
                var responseString = await response.Content.ReadAsStringAsync();

                // Desserealiza o conteúdo recebido relativo ao pedido HTTP à API
                var data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions);

                // Retorna os dados obtidos
                return (data ?? Activator.CreateInstance<T>(), null);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Log o erro ou trata conforme necessário
                    string errorMessage = "Unauthorized";
                    _logger.LogWarning(errorMessage);

                    return (default, errorMessage);
                }

                string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
                _logger.LogError(generalErrorMessage);
                return (default, generalErrorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            // Log o erro ou trata conforme necessário
            string errrMessage = $"Erro de requisição HTTP: {ex.Message}";
            _logger.LogError(errrMessage);

            return (default, errrMessage);
        }
        catch (JsonException ex)
        {
            // Log o erro ou trata conforme necessário
            string errorMessage = $"Erro de desserialização JSON: {ex.Message}";
            _logger.LogError(ex.Message);

            return (default, errorMessage);
        }
        catch (Exception ex)
        {            
            // Log o erro ou trata conforme necessário
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(ex.Message);

            return (default, errorMessage);
        }
    }

    // General Put Function to comunicate with API
    private async Task<HttpResponseMessage> PutRequest(string uri, HttpContent content)
    {
        var enderecoUrl = AppConfig.BaseUrl + uri;

        try
        {
            // Adiciona o token ao cabeçalho para autenticação
            await AddAuthorizationHeader();

            var response = await _httpClient.PutAsync(enderecoUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Log o erro ou trata conforme necessário
                    string errorMessage = "Unauthorized";
                    _logger.LogWarning(errorMessage);

                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                else
                {
                    // Log o erro ou trata conforme necessario
                    _logger.LogError($"Erro ao enviar requisição PUT para {uri}!");

                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
        }
        catch (Exception ex)
        {
            // Log o erro ou trata conforme necessario
            _logger.LogError($"Erro ao enviar requisição Put para {uri}: {ex.Message}");

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }

    // General Post Function to comunicate with API
    private async Task<HttpResponseMessage> PostRequest(string uri, HttpContent content)
    {
        var enderecoURL = AppConfig.BaseUrl + uri;

        try
        {
            //// Adiciona o token ao cabeçalho para autenticação
            //await AddAuthorizationHeader();

            var response = await _httpClient.PostAsync(enderecoURL, content);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Log o erro ou trata conforme necessário
                    string errorMessage = "Unauthorized";
                    _logger.LogWarning(errorMessage);

                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                else
                {
                    // Log o erro ou trata conforme necessario
                    _logger.LogError($"Erro ao enviar requisição Post para {uri}!");

                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }


        }
        catch (Exception ex)
        {
            // Log o erro ou trata conforme necessario
            _logger.LogError($"Erro ao enviar requisição POST para {uri}: {ex.Message}");

            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }

    // Specific Methods defined in the Interface
    // Obter as categorias

    public async Task<List<Categoria>> GetCategorias()
    {

        string endpoint = $"api/Categorias";

        try
        {
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string content = "";

                content = await httpResponseMessage.Content.ReadAsStringAsync();
                categorias = JsonSerializer.Deserialize<List<Categoria>>(content, _serializerOptions)!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return null;
        }

        return categorias;
    }

    // Obter os produtos
    public async Task<(List<ProdutoDTO>?Produtos, string? ErrorMessage)> GetProdutos(string tipoProduto, string categoriaId)
    {
        string endpoint = $"api/Produtos?tipoProduto={tipoProduto}&categoriaId={categoriaId}";

        return await GetAsync<List<ProdutoDTO>>(endpoint);
    }
    //public async Task<IEnumerable<ProdutoDTO>> GetAllProdutos()
    //{
    //    string endpoint = $"api/Produtos?tipoProduto=todos";

    //    HttpResponseMessage response = await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

    //    var responseString = await response.Content.ReadAsStringAsync();

    //    IEnumerable<ProdutoDTO> data = JsonSerializer.Deserialize<IEnumerable<ProdutoDTO>>(responseString, _serializerOptions);

    //    return data;
    //}
    public async Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria)
    {
        string endpoint = "";

        if (produtoTipo == "categoria" && IdCategoria != null)
        {
            endpoint = $"api/Produtos?tipoProduto=categoria&categoriaId={IdCategoria}";
        }
        //else if (produtoTipo == "detalhe" && IdCategoria != null)
        //{
        //    endpoint = $"api/Produtos?tipoProduto=detalhe";
        //}
        else if (produtoTipo == "promocao")
        {
            endpoint = $"api/Produtos?tipoProduto=promocao";
        }
        else if (produtoTipo == "maisvendido")
        {
            endpoint = $"api/Produtos?tipoProduto=maisvendido";
        }
        else if (produtoTipo == "todos")
        {
            endpoint = $"api/Produtos?tipoProduto=todos";
        }
        else
        {
            return null;
        }

        // Envia pedido para a API usando GetAsync
        try
        {
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string content = "";

                content = await httpResponseMessage.Content.ReadAsStringAsync();
                produtos = JsonSerializer.Deserialize<List<ProdutoDTO>>(content, _serializerOptions)!;         
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return null;
        }

        return produtos;
    }

    // Obtém detalhes de um produto
    public async Task<ProdutoDTO> GetDetalheProduto(int IdProduto)
    {
        string endpoint = $"api/Produtos/{IdProduto}";

        try
        {
            // Adiciona o token ao cabeçalho para autenticação
            await AddAuthorizationHeader();

            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string content = "";

                content = await httpResponseMessage.Content.ReadAsStringAsync();
                detalhesProduto = JsonSerializer.Deserialize<ProdutoDTO>(content, _serializerOptions)!;

                return detalhesProduto;
            }
            else return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    // Gerir Encomendas
    public async Task<(List<OrderByUser>?, string? ErrorMessage)> GetOrderByUser(int userId)
    {
        string endpoint = $"api/pedidos/PedidosPorUsuario/{userId}";
        return await GetAsync<List<OrderByUser>>(endpoint);
    }
    public async Task<(List<OrderDetail>?, string? ErrorMessage)> GetOrderDetails(int pedidoId)
    {
        string endpoint = $"api/pedidos/DetalhesPedido/{pedidoId}";
        return await GetAsync<List<OrderDetail>>(endpoint);
    }
    
    // Gerir Encomendas

    public async Task<(List<CarOrderItens>? CarOrderItems, string? ErrorMessage)> GetCarOrderItens(int usuarioId)
    {
        var endpoint = $"api/ItensCarrinhoCompra/{usuarioId}";
        return await GetAsync<List<CarOrderItens>>(endpoint);
    }

    public async Task<ApiResponse<bool>>AdicionaItemNoCarrinho(CarOrder carrinhoCompra)
    {
        try
        {
            var json = JsonSerializer.Serialize(carrinhoCompra, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/ItensCarrinhoCompra", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                };
            }
            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao adicionar item no carrinho de compras: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    public async Task<(bool Data, string? ErrorMessage)> ActualizaQuantidadeItemCarrinho(int produtoId, string acao)
    {
        try
        {
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await PutRequest($"api/ItensCarrinhoCompra?produtoId={produtoId}&acao={acao}", content);

            if (!response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            { 
                if(response.StatusCode== HttpStatusCode.Unauthorized)
                {
                    string errorMessage = "Unauthorized";
                    _logger.LogWarning(errorMessage);
                    return (false, errorMessage);
                }
                string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
                _logger.LogError(generalErrorMessage);
                return (false, generalErrorMessage);
            }
        }
        catch (HttpRequestException  ex)
        {
            string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
            _logger.LogError(errorMessage);
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(errorMessage);
            return (false, errorMessage);
        }
    }

    //private async Task<HttpResponseMessage>PutRequest(string uri, HttpContent content)
    //{
    //    var enderecoUrl = AppConfig.BaseUrl + uri;
    //    try
    //    {
    //        await AddAuthorizationHeader();
    //        var result = await _httpClient.PutAsync(enderecoUrl, content);
    //        return result;
    //    }
    //    catch (Exception ex) 
    //    {
    //        _logger.LogError($"Erro ao enviar requisição PUT para {uri}: { ex.Message}");
    //        return new HttpResponseMessage(HttpStatusCode.BadRequest);
    //    }
    //}

    public async Task<ApiResponse<bool>> ConfirmarPedido(EncomendaUserDTO pedido)
    {
        try
        {
            var json = JsonSerializer.Serialize(pedido, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/Pedidos", content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized
                    ? "Unauthorized"
                    : $"Erro ao enviar requisição HTTP: {response.StatusCode}";
                
                _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = errorMessage
                };
            }
            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao confirmar pedido: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    // Carrinhos
    // Gere Carrinhos
    public async Task<ApiResponse<bool>> AdicionaItemNoCarrinho(ItemCarrinhoDTO carrinhoCompra)
    {
        try
        {
            await AddAuthorizationHeader();

            var json = JsonSerializer.Serialize(carrinhoCompra, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/ItensCarrinhoCompra", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                };
            }
            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao adicionar item no carrinho de compras: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<List<ItemCarrinhoDTO>>> GetCarrinhoCompras()
    {
        try
        {
            await AddAuthorizationHeader();

            string endpoint = $"api/Carrinhos";

            var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                List<ItemCarrinhoDTO>? data = null;
                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonSerializer.Deserialize<List<ItemCarrinhoDTO>>(responseString, _serializerOptions);
                }
                else
                {
                    data = new List<ItemCarrinhoDTO>(); // Carrinho vazio
                }

                return new ApiResponse<List<ItemCarrinhoDTO>> { Data = data };
            }
            else
            {
                string errorMessage = $"Erro ao obter carrinho de compras: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<List<ItemCarrinhoDTO>> { ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao obter carrinho de compras: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<List<ItemCarrinhoDTO>> { ErrorMessage = errorMessage };
        }
    }

    public async Task<ApiResponse<bool>> AdicionarProdutoCarrinho(ItemCarrinhoDTO itemCarrinhoCompra)
    {
        try
        {
            await AddAuthorizationHeader();

            var json = JsonSerializer.Serialize(itemCarrinhoCompra, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/Carrinhos", content);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao adicionar produto ao carrinho: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao adicionar produto ao carrinho: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    public async Task<ApiResponse<bool>> AtualizarQuantidadeProdutoCarrinho(int produtoId, double novaQuantidade)
    {
        try
        {
            if (novaQuantidade <= 0)
            {
                return new ApiResponse<bool> { Data = false, ErrorMessage = "Quantidade deve ser maior que zero." };
            }

            string endpoint = $"api/Carrinhos/{produtoId}?novaQuantidade={novaQuantidade}";
            await AddAuthorizationHeader();

            var response = await _httpClient.PutAsync(AppConfig.BaseUrl + endpoint, null);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao atualizar quantidade do produto no carrinho: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao atualizar quantidade no carrinho: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    public async Task<ApiResponse<bool>> RemoverProdutoCarrinho(int produtoId)
    {
        try
        {
            string endpoint = $"api/Carrinhos/{produtoId}";
            await AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(AppConfig.BaseUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao remover produto do carrinho: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao remover produto do carrinho: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    public async Task<ApiResponse<bool>> LimparCarrinhoCompras()
    {
        try
        {
            string endpoint = $"api/Carrinhos";
            await AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(AppConfig.BaseUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao limpar carrinho: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao limpar carrinho: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }


    // *************** Gerir Favoritos ******************
    public async Task<ApiResponse<List<ProdutoFavorito>>> GetFavoritos()
    {
        try
        {
            string endpoint = $"api/Favoritos";

            await AddAuthorizationHeader();
            HttpResponseMessage response = await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<ProdutoFavorito>>(responseString, _serializerOptions);
                return new ApiResponse<List<ProdutoFavorito>> { Data = data };
            }
            else
            {
                string errorMessage = $"Erro ao obter favoritos: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<List<ProdutoFavorito>> { ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao obter favoritos: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<List<ProdutoFavorito>> { ErrorMessage = errorMessage };
        }
    }

    public async Task<ApiResponse<bool>> AdicionarFavorito(int produtoId)
    {
        try
        {
            var endpoint = $"api/Favoritos/{produtoId}";

            // Adiciona o token ao cabeçalho
            await AddAuthorizationHeader();

            // Faz a requisição POST
            var response = await PostRequest(endpoint, new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao adicionar favorito: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (HttpRequestException ex)
        {
            string errorMessage = $"Erro de requisição HTTP ao adicionar favorito: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao adicionar favorito: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    public async Task<(bool Data, string? ErrorMessage)> ActualizaFavorito(string acao, int produtoId)
    {
        try
        {
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await FavoritosPutRequest($"api/Favoritos/{produtoId}/{acao}", content);

            if (!response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string errorMessage = "Unauthorized";
                    _logger.LogWarning(errorMessage);
                    return (false, errorMessage);
                }
                string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
                _logger.LogError(generalErrorMessage);
                return (false, generalErrorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
            _logger.LogError(errorMessage);
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(errorMessage);
            return (false, errorMessage);
        }
    }

    private async Task<HttpResponseMessage> FavoritosPutRequest(string uri, HttpContent content)
    {
        var enderecoUrl = AppConfig.BaseUrl + uri;
        try
        {
            //  AddAuthorizationHeader();
            var result = await _httpClient.PutAsync(enderecoUrl, content);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao enviar requisição PUT para {uri}: {ex.Message}");
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }

    public async Task<ApiResponse<bool>> RemoverFavorito(int produtoId)
    {
        try
        {
            var endpoint = $"api/Favoritos/{produtoId}";

            await AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync(AppConfig.BaseUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao remover favorito: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (HttpRequestException ex)
        {
            string errorMessage = $"Erro de requisição HTTP ao remover favorito: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao remover favorito: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    // Orders

    // User Orders
    public async Task<ApiResponse<List<Encomenda>>> ObterEncomendasDoUserAsync()
    {
        try
        {
            await AddAuthorizationHeader();

            // GET api/Encomendas
            var endpoint = $"{AppConfig.BaseUrl}api/Encomendas";
            var response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                // Desserializar para List<Encomenda>
                var lista = JsonSerializer.Deserialize<List<Encomenda>>(responseString, _serializerOptions);

                return new ApiResponse<List<Encomenda>>
                {
                    Data = lista,
                    ErrorMessage = null
                };
            }
            else
            {
                // Ler eventual corpo de erro
                var errorBody = await response.Content.ReadAsStringAsync();
                var errorMessage = $"Erro ao obter encomendas: {response.ReasonPhrase} - {errorBody}";
                _logger.LogError(errorMessage);

                return new ApiResponse<List<Encomenda>>
                {
                    Data = null,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro inesperado ao obter encomendas: {ex.Message}";
            _logger.LogError(errorMessage);

            return new ApiResponse<List<Encomenda>>
            {
                Data = null,
                ErrorMessage = errorMessage
            };
        }
    }

    // Submeter Encomenda
    public async Task<ApiResponse<CriarEncomendaResponse>> CriarEncomendaAsync(EncomendaDTO encomendaDTO)
    {
        try
        {
            await AddAuthorizationHeader();

            var json = JsonSerializer.Serialize(encomendaDTO, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var endpoint = AppConfig.BaseUrl + "api/Encomendas";
            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // Aqui desserializamos a resposta para a classe 'CriarEncomendaResponse'
                var encomendaResponse = JsonSerializer.Deserialize<CriarEncomendaResponse>(responseString, _serializerOptions);

                return new ApiResponse<CriarEncomendaResponse>
                {
                    Data = encomendaResponse,
                    ErrorMessage = null
                };
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var errorMessage = $"Erro ao criar encomenda: {response.ReasonPhrase} - {errorBody}";
                _logger.LogError(errorMessage);

                return new ApiResponse<CriarEncomendaResponse>
                {
                    Data = null,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao criar encomenda: {ex.Message}";
            _logger.LogError(errorMessage);

            return new ApiResponse<CriarEncomendaResponse>
            {
                Data = null,
                ErrorMessage = errorMessage
            };
        }
    }

    // Pagar Encomenda
    public async Task<ApiResponse<string>> PagarEncomendaAsync(int idEncomenda)
    {
        try
        {
            await AddAuthorizationHeader();

            var endpoint = $"{AppConfig.BaseUrl}api/Encomendas/{idEncomenda}/pagar";

            var response = await _httpClient.PutAsync(endpoint, null);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                return new ApiResponse<string>
                {
                    Data = responseString,
                    ErrorMessage = null
                };
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var errorMessage = $"Erro ao pagar encomenda: {response.ReasonPhrase} - {errorBody}";
                _logger.LogError(errorMessage);

                return new ApiResponse<string>
                {
                    Data = null,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro inesperado ao pagar encomenda: {ex.Message}";
            _logger.LogError(errorMessage);

            return new ApiResponse<string>
            {
                Data = null,
                ErrorMessage = errorMessage
            };
        }
    }

    // Cancelar Encomenda
    public async Task<ApiResponse<CancelarEncomendaResponse>> CancelarEncomendaAsync(int idEncomenda)
    {
        try
        {
            await AddAuthorizationHeader();

            var endpoint = $"{AppConfig.BaseUrl}api/Encomendas/{idEncomenda}/cancelar";

            var response = await _httpClient.PostAsync(endpoint, null);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // Mapear o JSON para a classe CancelarEncomendaResponse
                var responseData = JsonSerializer.Deserialize<CancelarEncomendaResponse>(responseString);

                return new ApiResponse<CancelarEncomendaResponse>
                {
                    Data = responseData,
                    ErrorMessage = null
                };
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();

                //  extrair "ErrorMessage" do corpo da resposta
                try
                {
                    using var jsonDoc = JsonDocument.Parse(errorBody);
                    var errorMessage = jsonDoc.RootElement.GetProperty("ErrorMessage").GetString();

                    return new ApiResponse<CancelarEncomendaResponse>
                    {
                        Data = null,
                        ErrorMessage = errorMessage
                    };
                }
                catch
                {
                    // Caso o erro não seja JSON, retorna mensagem genérica
                    var genericErrorMessage = $"Erro ao cancelar encomenda: {response.ReasonPhrase}";
                    _logger.LogError(genericErrorMessage);

                    return new ApiResponse<CancelarEncomendaResponse>
                    {
                        Data = null,
                        ErrorMessage = genericErrorMessage
                    };
                }
            }
        }
        catch (Exception ex)
        {

            var errorMessage = $"Erro inesperado ao cancelar encomenda: {ex.Message}";
            _logger.LogError(errorMessage);

            return new ApiResponse<CancelarEncomendaResponse>
            {
                Data = null,
                ErrorMessage = errorMessage
            };
        }
    }

    /*public class CancelarEncomendaResponse
    {
        public string Message { get; set; } = string.Empty;
        public int EncomendaId { get; set; }
    }

    public class CriarEncomendaResponse
    {
        public string Message { get; set; }
        public int EncomendaId { get; set; }
    }*/


    // ****************** Utilizadores ********************
    public async Task<ApiResponse<bool>> RegistarUtilizador(RegisterModel registerModel)
    {
        try
        {
            var json = JsonSerializer.Serialize(registerModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/Utilizadores/RegistarUser", content);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                };
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> Login(LoginModel login)
    {
        try
        {
            string endpoint = $"{AppConfig.BaseUrl.TrimEnd('/')}/api/Utilizadores/LoginUser";

            var json = JsonSerializer.Serialize(login, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode} - {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }

            var responseString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
            {
                _logger.LogError("A resposta da API está vazia.");
                return new ApiResponse<bool> { Data = false, ErrorMessage = "A API não retornou um token." };
            }

            // Desserializar a resposta para o modelo Token DTO
            var token = JsonSerializer.Deserialize<Token>(responseString, _serializerOptions);

            if (token != null && !string.IsNullOrEmpty(token.AccessToken))
            {
                // GUARDAR NO SINGLETON CRIADO NO PROGRAM CS
                await _userSessionService.Login(token);

                _logger.LogInformation($"Token recebido: {token.AccessToken}");
                _logger.LogInformation($"Utilizador: {(await _userSessionService.GetToken()).UtilizadorNome} com Login Efetuado ? {await _userSessionService.IsUserLoggedIn()}");
                return new ApiResponse<bool> { Data = true };
            }

            return new ApiResponse<bool> { Data = false, ErrorMessage = "Token inválido ou não recebido." };
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado no login: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    // User State
    public async Task<ApiResponse<UtilizadorEstadoDTO>> VerificarEstadoUtilizador(string id)
    {
        try
        {
            string endpoint = $"api/Utilizadores/CheckIfActive?id={id}";

            // Adicionar o token ao cabeçalho para autenticação
            await AddAuthorizationHeader();

            // Fazer a requisição GET
            var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var estadoUtilizador = JsonSerializer.Deserialize<UtilizadorEstadoDTO>(responseString, _serializerOptions);

                return new ApiResponse<UtilizadorEstadoDTO>
                {
                    Data = estadoUtilizador,
                    ErrorMessage = null
                };
            }
            else
            {
                var errorMessage = $"Erro ao verificar estado do utilizador: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);

                return new ApiResponse<UtilizadorEstadoDTO>
                {
                    Data = null,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro inesperado ao verificar estado do utilizador: {ex.Message}";
            _logger.LogError(errorMessage);

            return new ApiResponse<UtilizadorEstadoDTO>
            {
                Data = null,
                ErrorMessage = errorMessage
            };
        }
    }

    // User Data
    public async Task<ApiResponse<UtilizadorDTO>> ObterDadosUtilizadorAsync()
    {
        try
        {
            string endpoint = "api/Utilizadores/ObterDadosUtilizador";

            await AddAuthorizationHeader();

            var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var utilizador = JsonSerializer.Deserialize<UtilizadorDTO>(responseString, _serializerOptions);
                return new ApiResponse<UtilizadorDTO> { Data = utilizador };
            }
            else
            {
                string errorMessage = $"Erro ao obter dados do utilizador: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<UtilizadorDTO> { Data = null, ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao obter dados do utilizador: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<UtilizadorDTO> { Data = null, ErrorMessage = errorMessage };
        }
    }

    // User Data Edit
    public async Task<ApiResponse<bool>> EditarDadosUtilizadorAsync(UtilizadorDTO utilizadorDto)
    {
        try
        {
            string endpoint = "api/Utilizadores/EditarDadosUtilizador";

            var json = JsonSerializer.Serialize(utilizadorDto, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await AddAuthorizationHeader();

            var response = await _httpClient.PutAsync(AppConfig.BaseUrl + endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Data = true };
            }
            else
            {
                string errorMessage = $"Erro ao editar dados do utilizador: {response.ReasonPhrase}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado ao editar dados do utilizador: {ex.Message}";
            _logger.LogError(errorMessage);
            return new ApiResponse<bool> { Data = false, ErrorMessage = errorMessage };
        }
    }

    internal async Task<ApiResponse<bool>> UploadImagemUtilizador(byte[] imageArray)
    {
        try
        {
            var content = new MultipartFormDataContent();

            content.Add(new ByteArrayContent(imageArray), "imagem", "image.jpg");

            var response = await PostRequest("api/Usuarios/uploadfoto", content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized ? "Unauthorized"
                    : "Erro ao enviar requisição HTTP: {response.StatusCode}";

                _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");

                return new ApiResponse<bool> { ErrorMessage = errorMessage };
            }
            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao fazer o upload da imagem do utilizador: {ex.Message}");

            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }
    public async Task<(ImagemPerfil? ImagemPerfil, string? ErrorMessage)> GetImagemPerfilUsuario()
    {
        string endpoint = "api/Usuarios/imagemperfil";

        return await GetAsync<ImagemPerfil>(endpoint);
    }


}
