using GestaoLoja2.Entitles;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestaoLoja2.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {


        public DbSet<categoria> Categorias { get; set; }
        public DbSet<produto> Produtos { get; set; }
        public DbSet<ModoEntrega> ModosEntrega { get; set; }
     
    }
}
