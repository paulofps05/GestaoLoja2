namespace RESTfulAPIPWeb.DTO
{
    public class RegisterModel
    {
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public long NIF { get; set; }
        public string Telemovel { get; set; } // This will be mapped to PhoneNumber
        public string Rua { get; set; }
        public string Localidade { get; set; } // This will be mapped to Localidade1
        public string CodigoPostal { get; set; } // This will be mapped to Localidade2
        public string Pais { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
