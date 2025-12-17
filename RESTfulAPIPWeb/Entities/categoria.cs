using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace RESTfulAPIPWeb.Entities;
    public class categoria
{
 public int Id { get; set; }

  

    [Required(ErrorMessage = "O nome da categoria é obrigatória!")]
    public string? Nome { get; set; } = string.Empty;
    public int? Ordem { get; set; }
    public string? UrlImagem { get; set; }
    public byte[]? Imagem { get; set; }

    
    [NotMapped]
    [JsonIgnore]
    public IFormFile? ImagemFile { get; set; }

}