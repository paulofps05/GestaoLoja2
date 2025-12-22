using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RESTfulAPIPWeb.DTO;
using RESTfulAPIPWeb.Data;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(IConfiguration config, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
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
            var utilizadorExiste = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == utilizador.Email);

            if (utilizadorExiste != null)
            {
                return BadRequest("Já existe um utilizador com este email");
            }

            // Criar o novo utilizador mapeando para os campos existentes na BD
            var novoUtilizador = new ApplicationUser
            {
                UserName = utilizador.Email,
                Email = utilizador.Email,
                Nome = utilizador.Nome,
                Apelido = utilizador.Apelido,
                NIF = utilizador.NIF,
                Rua = utilizador.Rua,
                Localidade1 = utilizador.Localidade, // Mapeado
                Localidade2 = utilizador.CodigoPostal, // Mapeado
                Pais = utilizador.Pais,
                PhoneNumber = utilizador.Telemovel, // Mapeado
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(novoUtilizador, utilizador.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(novoUtilizador, "Cliente");
                return StatusCode(StatusCodes.Status201Created);
            }
            
            return BadRequest(result.Errors);
        }

        // User Login
        [HttpPost("[action]")]
        public async Task<IActionResult> LoginUser([FromBody] UtilizadorLoginModel utilizador)
        {
            var utilizadorAtual = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == utilizador.Email);

            if (utilizadorAtual is null)
            {
                return NotFound("Utilizador não encontrado");
            }

            var result = await _signInManager.PasswordSignInAsync(utilizador.Email, utilizador.Password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var tempUser = await _userManager.FindByEmailAsync(utilizador.Email);
                var userRoles = await _userManager.GetRolesAsync(tempUser);

                if (!userRoles.Contains("Cliente") && !userRoles.Contains("Admin"))
                {
                    return Forbid("Utilizador não autorizado a fazer login.");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, utilizador.Email),
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
        public async Task<IActionResult> CheckIfActive([FromQuery] string id)
        {
            var idDoToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idDoToken) || !id.Equals(idDoToken, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized("Acesso negado.");
            }

            var utilizador = await _userManager.FindByIdAsync(id);
            if (utilizador is null) return NotFound();

            // Como o campo Estado não existe na BD, assumimos activo se o utilizador existe
            return Ok(new { UtilizadorId = utilizador.Id, Nome = utilizador.Nome, IsActive = true });
        }

        // User data
        [Authorize(Roles = "Cliente", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("[action]")]
        public async Task<IActionResult> ObterDadosUtilizador()
        {
            var idDoToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idDoToken)) return Unauthorized();

            var utilizador = await _userManager.FindByIdAsync(idDoToken);
            if (utilizador is null) return NotFound();

            return Ok(new UtilizadorDTO
            {
                Email = utilizador.Email,
                Nome = utilizador.Nome,
                Apelido = utilizador.Apelido,
                Rua = utilizador.Rua,
                Localidade = utilizador.Localidade1,
                CodigoPostal = utilizador.Localidade2,
                Pais = utilizador.Pais,
                Telemovel = utilizador.PhoneNumber,
                NIF = utilizador.NIF
            });
        }

        // User data edit
        [Authorize(Roles = "Cliente", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("[action]")]
        public async Task<IActionResult> EditarDadosUtilizador([FromBody] UtilizadorDTO utilizadorDto)
        {
            var idDoToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idDoToken)) return Unauthorized();

            var utilizador = await _userManager.FindByIdAsync(idDoToken);
            if (utilizador is null) return NotFound();

            utilizador.Nome = utilizadorDto.Nome ?? utilizador.Nome;
            utilizador.Apelido = utilizadorDto.Apelido ?? utilizador.Apelido;
            utilizador.Rua = utilizadorDto.Rua ?? utilizador.Rua;
            utilizador.Localidade1 = utilizadorDto.Localidade ?? utilizador.Localidade1;
            utilizador.Localidade2 = utilizadorDto.CodigoPostal ?? utilizador.Localidade2;
            utilizador.Pais = utilizadorDto.Pais ?? utilizador.Pais;
            utilizador.PhoneNumber = utilizadorDto.Telemovel ?? utilizador.PhoneNumber;
            utilizador.NIF = utilizadorDto.NIF ?? utilizador.NIF;

            var result = await _userManager.UpdateAsync(utilizador);
            if (!result.Succeeded) return BadRequest("Erro ao atualizar.");

            return Ok(new { Message = "Dados atualizados com sucesso." });
        }

        // Models integrados no controlador para facilitar o copy-paste solicitado
        public class UtilizadorLoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegisterModel
        {
            public string Nome { get; set; }
            public string Apelido { get; set; }
            public long? NIF { get; set; }
            public string Telemovel { get; set; }
            public string Rua { get; set; }
            public string Localidade { get; set; }
            public string CodigoPostal { get; set; }
            public string Pais { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
