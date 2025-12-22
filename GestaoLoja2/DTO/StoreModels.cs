namespace GestaoLoja2.DTO;

public class ProdutoDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Detalhe { get; set; }
    public string UrlImagem { get; set; }
    public byte[] Imagem { get; set; }
    public decimal Preco { get; set; }
    public bool Promocao { get; set; }
    public bool MaisVendido { get; set; }
    public decimal EmStock { get; set; }
    public bool Disponivel { get; set; }
    public string Origem { get; set; }
    public int CategoriaId { get; set; }
}

public class ItemCarrinhoDTO
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; }
    public double PrecoUnitario { get; set; }
    public double Quantidade { get; set; }
    public double ValorTotal => PrecoUnitario * Quantidade;
    public string? UrlImagem { get; set; }
}

public class ProdutoFavorito
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; }
    public string UrlImagem { get; set; }
    public decimal Preco { get; set; }
}
