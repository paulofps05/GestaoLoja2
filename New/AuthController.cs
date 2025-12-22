using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using RestfulAPIWeb.DTO;
using RestfulAPIWeb.Data;

[Route("api/[controller]")]
[ApiController]
public class UtilizadoresController : ControllerBase
{
    private readonly IConfiguration _config;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UtilizadoresController(IConfiguration config, UserManager<ApplicationUser> 
        userManager, SignInManager<ApplicationUser> signInManager)
    {
        _config = config;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // User Register

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistarUser([FromBody] RegisterModel utilizador)
    {
        var utilizadorExiste = await _userManager.Users.FirstOrDefaultAsync(u => u.Email 
        == utilizador.Email);

        if (utilizadorExiste != null)
        {
            return BadRequest("Já existe um utilizador com este email");
        }

        // Criar o novo utilizador
        var novoUtilizador = new ApplicationUser
        {
            UserName = utilizador.Email,
            Email = utilizador.Email,
            Nome = utilizador.Nome,
            Apelido = utilizador.Apelido,
            NIF = utilizador.NIF,
            Rua = utilizador.Rua,
            Localidade = utilizador.Localidade,
            CodigoPostal = utilizador.CodigoPostal,
            Cidade = utilizador.Cidade,
            Pais = utilizador.Pais,
            Telemovel = utilizador.Telemovel,
            DataRegisto = DateTime.UtcNow,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };

        await _userManager.CreateAsync(novoUtilizador, utilizador.Password);
        await _userManager.AddToRoleAsync(novoUtilizador, "Cliente");

 // Console.WriteLine("Email: " + utilizador.Email + " Password: " + utilizador.Password);

        return StatusCode(StatusCodes.Status201Created);
    }

    // User Login
    [HttpPost("[action]")]
    public async Task<IActionResult> LoginUser([FromBody] UtilizadorLoginModel utilizador)
    {
        var utilizadorAtual = await _userManager.Users.FirstOrDefaultAsync(u =>
                                 u.Email == utilizador.Email);

        if (utilizadorAtual is null)
        {
            return NotFound("Utilizador não encontrado");
        }

        // ************ Logar com Identity
        var result = await _signInManager.PasswordSignInAsync(utilizador.Email, 
            utilizador.Password, false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var tempUser = await _userManager.FindByEmailAsync(utilizador.Email);
            var userRoles = await _userManager.GetRolesAsync(tempUser);

            // Regra de negócio: Apenas users com o role "Cliente" se podem logar no front end
            if (!userRoles.Contains("Cliente"))
            {
                return Forbid("Utilizador não autorizado a fazer login.");
            }

            // Gerar e gravar o token JWT
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email , utilizador.Email),
                new Claim(ClaimTypes.NameIdentifier, tempUser.Id),
                new Claim(ClaimTypes.Role, userRoles[0]!)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new ObjectResult(new
            {
                accesstoken = jwt,
                tokentype = "bearer",
                utilizadorid = utilizadorAtual.Id,
                utilizadornome = utilizadorAtual.Nome
            });
        }
        else
        {
            return BadRequest("Erro: Login Inválido!");
        }
    }

    // Verifica o estado do user
    [Authorize(Roles = "Cliente", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckIfActive([FromQuery] string id)
    {
        var idDoToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idDoToken))
        {
            return Unauthorized("O token não contém um ID válido.");
        }

        if (!id.Equals(idDoToken, StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized("O ID fornecido não corresponde ao utilizador autenticado.");
        }
        var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (utilizador is null)
        {
            return NotFound("Utilizador não encontrado.");
        }

        var estaAtivo = utilizador.Estado == EstadoUtilizador.Activo;

        return Ok(new
        {
            UtilizadorId = utilizador.Id,
            Nome = utilizador.Nome,
            Apelido = utilizador.Apelido,
            IsActive = estaAtivo
        });
    }

    // User data
    [Authorize(Roles = "Cliente", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterDadosUtilizador()
    {
        var idDoToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idDoToken))
        {
            return Unauthorized("O token não contém um ID válido.");
        }

        var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == idDoToken);

        if (utilizador is null)
        {
            return NotFound("Utilizador não encontrado.");
        }

        var utilizadorDto = new UtilizadorDTO
        {
            Email = utilizador.Email,
            Nome = utilizador.Nome,
            Apelido = utilizador.Apelido,
            Rua = utilizador.Rua,
            Localidade = utilizador.Localidade,
            Cidade = utilizador.Cidade,
            Pais = utilizador.Pais,
            CodigoPostal = utilizador.CodigoPostal,
            Telemovel = utilizador.Telemovel,
            NIF = utilizador.NIF,
            DataRegisto = utilizador.DataRegisto,
            Estado = utilizador.Estado.ToString()
        };

        return Ok(utilizadorDto);
    }

    // User data edit
    [Authorize(Roles = "Cliente", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EditarDadosUtilizador([FromBody] UtilizadorDTO utilizadorDto)
    {
        var idDoToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idDoToken))
        {
            return Unauthorized("O token não contém um ID válido.");
        }

        var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == idDoToken);

        if (utilizador is null)
        {
            return NotFound("Utilizador não encontrado.");
        }

        utilizador.Nome = utilizadorDto.Nome ?? utilizador.Nome;
        utilizador.Apelido = utilizadorDto.Apelido ?? utilizador.Apelido;
        utilizador.Rua = utilizadorDto.Rua ?? utilizador.Rua;
        utilizador.Localidade = utilizadorDto.Localidade ?? utilizador.Localidade;
        utilizador.Cidade = utilizadorDto.Cidade ?? utilizador.Cidade;
        utilizador.Pais = utilizadorDto.Pais ?? utilizador.Pais;
        utilizador.CodigoPostal = utilizadorDto.CodigoPostal ?? utilizador.CodigoPostal;
        utilizador.Telemovel = utilizadorDto.Telemovel ?? utilizador.Telemovel;
        utilizador.NIF = utilizadorDto.NIF ?? utilizador.NIF;

        var resultado = await _userManager.UpdateAsync(utilizador);

        if (!resultado.Succeeded)
        {
            return BadRequest("Erro ao atualizar os dados do utilizador.");
        }

        return Ok(new
        {
            Message = "Dados do utilizador atualizados com sucesso."
        });
    }

    // User models
    public class UtilizadorLoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public long NIF { get; set; }
        public string Telemovel { get; set; }
        public string Rua { get; set; }
        public string Localidade { get; set; }
        public string CodigoPostal { get; set; }
        public string Cidade { get; set; }
        public string Pais { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
