using Microsoft.AspNetCore.Identity;

namespace GestaoLoja2.Data;


public static class inicializacao
{

    public static async Task CriaDadosIniciais(
     UserManager<ApplicationUser> userManager,
     RoleManager<IdentityRole> roleManager)
    {
        // Adicionar default roles
        string[] roles = ["Admin", "Gestor", "Cliente"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                IdentityRole roleRole = new IdentityRole(role);
                await roleManager.CreateAsync(roleRole);
            }
        }

        // Adicionar um utilizador Admin por defeito
        var defaultUser = new ApplicationUser
        {
            UserName = "admin@localhost.com",
            Email = "admin@localhost.com",
            Nome = "Administrador",
            Apelido = "Local",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };
        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Is3C..00");
                await userManager.AddToRoleAsync(defaultUser, "Admin");
            }
        }
    }
}